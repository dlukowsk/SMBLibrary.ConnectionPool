using System;
using System.Collections.Concurrent;
using Smarsh.Web.RetrievalService.Repositories.SMB;
using SMBConnectionPooler.Helpers;


namespace SMBConnectionPooler.Repositories.SMB
{
    public interface ISMB2ConnectionPool
    {
        SMB2ConnectionClient CheckOut(string host, string share);

        void CheckIn(SMB2ConnectionClient connectionClient);
    }

    public class SMB2ConnectionPool : ISMB2ConnectionPool
    {
        private static readonly ConcurrentDictionary<string, SMB2ConnectionSource> _smb2ConnectionSources = new ConcurrentDictionary<string, SMB2ConnectionSource>();
        private readonly string _domain;
        private readonly string _username;
        private readonly string _password;

        public SMB2ConnectionPool(string domain, string username, string password)
        {
            _domain = domain;
            _username = username;
            _password = password;
        }

        public SMB2ConnectionClient CheckOut(string host, string share)
        {
            SMB2ConnectionSource source = getOrCreateSMB2ConnectionSource(host, share);
            Console.WriteLine($"SMB2ConnectionPool.CheckOut SourceHost:{host} Share:{share} Hash:{source.TestGuid}");
            return source.CheckOut();
        }

        public void CheckIn(SMB2ConnectionClient connectionClient)
        {
            SMB2ConnectionSource source = getOrCreateSMB2ConnectionSource(connectionClient.Host, connectionClient.Share);
            source.CheckIn(connectionClient);
        }

        private SMB2ConnectionSource getOrCreateSMB2ConnectionSource(string host, string share)
        {
            string key = SMB2ConnectionHelper.MakeKey(host, share);
            Console.WriteLine($"SMB2ConnectionPool.GetOrCreateConnectionSource Key:{key}");
            return _smb2ConnectionSources.GetOrAdd(key, (key) => new SMB2ConnectionSource(_domain,
                _username,
                _password,
                host,
                share
            ));
        }



    }
}