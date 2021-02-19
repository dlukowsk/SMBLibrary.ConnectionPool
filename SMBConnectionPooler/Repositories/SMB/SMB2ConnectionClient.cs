using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SMBConnectionPooler.Domain;
using SMBConnectionPooler.Helpers;
using SMBLibrary;
using SMBLibrary.Client;

using FileInformation = SMBConnectionPooler.Domain.FileInformation;


namespace SMBConnectionPooler.Repositories.SMB
{
    public interface ISMB2PoolClient
    {
        bool IsConnected { get; }
        string Key { get; }

        NTStatus CurrentConnectionStatus();

        void GetFileStream(NamedObjectStream stream, string uncPath, string contentFilename,
            long? startRange = null, long? endRange = null);

        Task<NamedObjectBytes> GetFileByes(string uncPath, string contentFilename,
            long? startRange = null, long? endRange = null);

        FileInformation GetFileInformation(string path, string filename);
    }
    public class SMB2ConnectionClient : SMB2Client, ISMB2PoolClient
    {
        private bool _connected;
        private NTStatus _status;
        private SMB2FileStore _smb2FileStore;
        private string _host;
        private string _share;

        public string Host => _host;
        public string Share => _share;

        public string Key
        {
            get { return SMB2ConnectionHelper.MakeKey(_host, _share); }
        }

        public bool IsConnected
        {
            get
            {
                return _status == NTStatus.STATUS_SUCCESS && _connected;
            }
        }

        //public bool InUse => _inUse;

        public SMB2ConnectionClient(string domain, string username, string password, string host, string share)
        {

            //INFO dotnet core was having an issue with Code Contracts and required a visual studio extension to resolve
            if (String.IsNullOrEmpty(domain))
                throw new ArgumentNullException("domainName is required to connect to unc host.");
            if (String.IsNullOrEmpty(username))
                throw new ArgumentNullException("username is required to connect to unc host.");
            if (String.IsNullOrEmpty(password))
                throw new ArgumentNullException("password is required to connect to unc host.");
            if (String.IsNullOrEmpty(host))
                throw new ArgumentNullException("uncHost is required to connect to unc host.");
            if (String.IsNullOrEmpty(share))
                throw new ArgumentNullException("uncShare is required to connect to unc host.");

            IPAddress ipAddress = Dns.GetHostAddresses(host)[0];
            _host = host;
            _share = share;
            _connected = base.Connect(ipAddress, SMBTransportType.DirectTCPTransport);
            if (_connected)
            {
                _status = Login(domain, username, password);
                if (_status == NTStatus.STATUS_SUCCESS)
                {
                    _smb2FileStore = TreeConnect(_share, out _status) as SMB2FileStore;
                    if (_smb2FileStore == null)
                        _connected = false;
                }
            }
        }

        public NTStatus CurrentConnectionStatus()
        {
            TreeConnect(_share, out _status);
            return _status;
        }


        public async Task<NamedObjectBytes> GetFileByes(string uncPath, string contentFilename,
            long? startRange = null, long? endRange = null)
        {
            Stopwatch timer = Stopwatch.StartNew();

            
            //Console.WriteLine($"SMB2ConnectionClient.GetFileBytes - CreateFile - PRECONNECT - THREADID:{this.GetHashCode()} NTStatus:{_status} FileStatus:NoConnection HOST:{_host} SHARE:{_share }PATH:{uncPath} FILENAME:{contentFilename} ");
            

            _smb2FileStore = TreeConnect(_share, out _status) as SMB2FileStore;
            if (_smb2FileStore == null)
            {
                _connected = false;

                return null;
            }



            object fileHandle = new object();
            FileStatus status;
            SMBLibrary.FileInformation fileInfo = null;
            var pathToFile = $"{uncPath}\\{contentFilename}";

            _status = _smb2FileStore.CreateFile(out fileHandle, out status, pathToFile,
                AccessMask.GENERIC_READ, 0, ShareAccess.Read, CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_NON_DIRECTORY_FILE, null);
            Console.WriteLine($"SMB2ConnectionClient.GetFileBytes - CreateFile- POSTCONNECT - THREADID:{this.GetHashCode()} NTStatus:{_status} FileStatus:{status} HOST:{_host} SHARE:{_share }PATH:{uncPath} FILENAME:{contentFilename} ");
            

            if (_status == NTStatus.STATUS_SUCCESS)
            {
                bool read = true;
                byte[] data = null;
                int start = (startRange != null && startRange.ToInt(0) >= 0) ? startRange.ToInt(0) : 0;
                int chunkSize = 65536;


                _smb2FileStore.GetFileInformation(out fileInfo, fileHandle,
                    FileInformationClass.FileStandardInformation);

                //is this a ranged call?
                if (startRange != null && endRange != null)
                {
                    chunkSize = ((endRange - startRange) < chunkSize) ? (int)(endRange - startRange + 1) : chunkSize;
                    int remainingBytesToRead = endRange.ToInt(0) - startRange.ToInt(0) + 1;

                    while (read)
                    {
                        _smb2FileStore.ReadFile(out var chunk, fileHandle, start, chunkSize);
                        data = data == null ? chunk : data.Concat(chunk).ToArray();
                        start += chunkSize;
                        remainingBytesToRead -= chunkSize;
                        chunkSize = (remainingBytesToRead < chunkSize) ? remainingBytesToRead : chunkSize;

                        if (remainingBytesToRead <= 0) read = false;
                    }

                    _smb2FileStore.CloseFile(fileHandle);
                    return new NamedObjectBytes()
                    {
                        FileBytes = data,
                        TotalFileLength = ((FileStandardInformation)fileInfo).EndOfFile,
                        RetrieveSpeedMs = timer.Elapsed.TotalMilliseconds
                    };

                }


                while (read)
                {
                    _smb2FileStore.ReadFile(out var chunk, fileHandle, start, chunkSize);
                    if (chunk == null)
                    {
                        Debugger.Break();
                    }
                    data = data == null ? chunk : data.Concat(chunk).ToArray();
                    start += chunkSize;
                    if (chunk == null || chunk.Length < chunkSize) read = false;
                }

                _smb2FileStore.CloseFile(fileHandle);
                
                return new NamedObjectBytes()
                {
                    FileBytes = data,
                    TotalFileLength = ((FileStandardInformation)fileInfo).EndOfFile,
                    RetrieveSpeedMs = timer.Elapsed.TotalMilliseconds
                };

            }

            return null;
        }


        public void GetFileStream(NamedObjectStream stream, string uncPath, string contentFilename,
            long? startRange = null, long? endRange = null)
        {
            object fileHandle = new object();
            FileStatus status;
            SMBLibrary.FileInformation fileInfo = null;
            var pathToFile = $"{uncPath}\\{contentFilename}";

            _status = _smb2FileStore.CreateFile(out fileHandle, out status, pathToFile,
                AccessMask.GENERIC_READ, 0, ShareAccess.Read, CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_NON_DIRECTORY_FILE, null);

            if (_status == NTStatus.STATUS_SUCCESS)
            {
                bool read = true;
                byte[] data = null;
                int start = (startRange != null && startRange.ToInt(0) >= 0) ? startRange.ToInt(0) : 0;
                int chunkSize = 65536;


                _smb2FileStore.GetFileInformation(out fileInfo, fileHandle,
                    FileInformationClass.FileStandardInformation);

                //populate the stream metadata
                stream.Name = contentFilename;

                //is this a ranged call?
                if (startRange != null && endRange != null)
                {
                    chunkSize = ((endRange - startRange) < chunkSize) ? (int)(endRange - startRange + 1) : chunkSize;
                    int remainingBytesToRead = endRange.ToInt(0) - startRange.ToInt(0) + 1;

                    while (read)
                    {
                        _smb2FileStore.ReadFile(out var chunk, fileHandle, start, chunkSize);
                        stream.Write(chunk);
                        start += chunkSize;
                        remainingBytesToRead -= chunkSize;
                        chunkSize = (remainingBytesToRead < chunkSize) ? remainingBytesToRead : chunkSize;

                        if (remainingBytesToRead <= 0) read = false;
                    }
                }


                while (read)
                {
                    _smb2FileStore.ReadFile(out var chunk, fileHandle, start, chunkSize);
                    stream.Write(chunk);
                    start += chunkSize;
                    if (chunk == null || chunk.Length < chunkSize) read = false;
                }

                stream.Position = 0;
                _smb2FileStore.CloseFile(fileHandle);
            }
        }


        public FileInformation GetFileInformation(string path, string filename)
        {
            object fileHandle = new object();
            FileStatus fileStatus;
            var pathToFile = $"{path}\\{filename}";

            Dictionary<string, string> pathParts = Regex
                .Match(pathToFile, @".*\\(?<parentFolder>[^\\]+)\\").Groups
                .Cast<Group>().ToDictionary(g => g.Name, g => g.Value.TrimEnd('/'));
            string parentfolder = (pathParts.Count > 1) ? pathParts["parentFolder"] : "";

            _status = _smb2FileStore.CreateFile(out fileHandle, out fileStatus, pathToFile,
                AccessMask.GENERIC_READ, 0, ShareAccess.Read, CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_NON_DIRECTORY_FILE, null);

            if (_status == NTStatus.STATUS_SUCCESS)
            {
                SMBLibrary.FileInformation fileInfo = null;
                _smb2FileStore.GetFileInformation(out fileInfo, fileHandle,
                    FileInformationClass.FileStandardInformation);

                return new FileInformation()
                {
                    Name = filename,
                    DirectoryInfo = new DirectoryInformation()
                    {
                        Name = parentfolder,
                        Exists = true,
                        FullPath = $"{_host}\\{_share}\\{path}"
                    },
                    DirectoryName = parentfolder,
                    Exists = true,
                    FullPath = $"{_host}\\{_share}\\{pathToFile}",
                    Length = ((FileStandardInformation)fileInfo).EndOfFile
                };
            }
            //There was an issue locating the file
            else
            {
                return new FileInformation()
                {
                    Name = filename,
                    DirectoryInfo = new DirectoryInformation()
                    {
                        Name = parentfolder,
                        Exists = false,
                        FullPath = $"{_host}\\{_share}\\{path}"
                    },
                    DirectoryName = parentfolder,
                    Exists = false,
                    FullPath = $"{_host}\\{_share}\\{pathToFile}",
                    Length = 0
                };
            }
        }
    }
}
