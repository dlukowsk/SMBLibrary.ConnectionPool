using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using SMBConnectionPooler.Domain;
using SMBConnectionPooler.Repositories.SMB;

namespace SMBConnectionPooler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int fetches = 10;
            string domain = "store.local";
            string username = "username";
            string password = "PASSWORD!";

            for (int i = 0; i < fetches; i++)
            {
                SMB2ConnectionPool pool = new SMB2ConnectionPool(domain, username, password);

                //Serial Establish the connections for each of the pools.
                foreach (var filePath in getFilePaths())
                {
                    var smbConnectionClient = pool.CheckOut(filePath.Host, filePath.Share);

                    NamedObjectBytes fileObjectBytes =
                        smbConnectionClient.GetFileByes(filePath.Path, filePath.Filename).Result;
                    File.WriteAllBytes($"c:\\tmp\\{Guid.NewGuid().ToString()}_{filePath.Filename}", fileObjectBytes.FileBytes);

                    pool.CheckIn(smbConnectionClient);
                }

                //Parallel 
                Parallel.ForEach(getFilePaths(), filePath =>
                {

                    var smbConnectionClient = pool.CheckOut(filePath.Host, filePath.Share);

                    NamedObjectBytes fileObjectBytes =
                        smbConnectionClient.GetFileByes(filePath.Path, filePath.Filename).Result;
                    File.WriteAllBytes($"c:\\tmp\\{Guid.NewGuid().ToString()}_{filePath.Filename}", fileObjectBytes.FileBytes);

                    pool.CheckIn(smbConnectionClient);

                });


                //Serial Loading of Parallel Task List.
                List<Task> tasks = new List<Task>();
                int pass = 0;
                foreach (var filePath in getFilePaths())
                {
                    tasks.Add(Task.Run(() =>
                    {

                        var smbConnectionClient = pool.CheckOut(filePath.Host, filePath.Share);

                        NamedObjectBytes fileObjectBytes =
                            smbConnectionClient.GetFileByes(filePath.Path, filePath.Filename).Result;
                        File.WriteAllBytes($"c:\\tmp\\{Guid.NewGuid().ToString()}_{filePath.Filename}", fileObjectBytes.FileBytes);

                        pool.CheckIn(smbConnectionClient);
                        pass++;
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        private static IEnumerable<FilePath> getFilePaths()
        {
            List<FilePath> filePathList = new List<FilePath>();
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_Primary", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "0ce715a9e51f4f2cb499fd7aa461ef70.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_Primary", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "01afdf37a093459c856937c6a99913b7.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_Primary", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "2bbee5ce9a084a4bae3110e004bc3c66.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_Primary", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "5e6772df88484272a2349981bebe99d4.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_DR", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "0ce715a9e51f4f2cb499fd7aa461ef70.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_DR", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "01afdf37a093459c856937c6a99913b7.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_DR", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "2bbee5ce9a084a4bae3110e004bc3c66.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_DR", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "5e6772df88484272a2349981bebe99d4.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_Primary", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "0ce715a9e51f4f2cb499fd7aa461ef70.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_Primary", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "01afdf37a093459c856937c6a99913b7.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_Primary", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "2bbee5ce9a084a4bae3110e004bc3c66.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_Primary", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "5e6772df88484272a2349981bebe99d4.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_DR", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "0ce715a9e51f4f2cb499fd7aa461ef70.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_DR", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "01afdf37a093459c856937c6a99913b7.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_DR", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "2bbee5ce9a084a4bae3110e004bc3c66.eml" });
            filePathList.Add(new FilePath() { Host = "dev-vault-01.store.local", Share = "Vault_DR", Path = "A\\EmailArchive\\EML\\424\\2016\\6\\16", Filename = "5e6772df88484272a2349981bebe99d4.eml" });

            return filePathList;
        }

        public class FilePath
        {
            public string Host { get; set; }
            public string Share { get; set; }
            public string Path { get; set; }
            public string Filename { get; set; }
        }
    }
}