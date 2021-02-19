using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SMBConnectionPooler.Repositories.SMB;
using SMBLibrary;

namespace Smarsh.Web.RetrievalService.Repositories.SMB
{
    public class SMB2ConnectionSource
    {
        private readonly ConcurrentBag<SMB2ConnectionClient> _connections;
        private readonly string _domain;
        private readonly string _username;
        private readonly string _password;
        private readonly string _host;
        private readonly string _share;

        private readonly int _maxConnections = 2;

        private int _count = 0;

        public string TestGuid => this.GetHashCode().ToString();

        public SMB2ConnectionSource(string domain, string username, string password, string host, string share)
        {
            // save all the info we need to create connections
            _connections = new ConcurrentBag<SMB2ConnectionClient>();
            _domain = domain;
            _username = username;
            _password = password;
            _host = host;
            _share = share;
        }

        public SMB2ConnectionClient CheckOut()
        {
            SMB2ConnectionClient client = null;
            if (_connections.TryTake(out client))
            {
                NTStatus status = client.CurrentConnectionStatus();
                Console.WriteLine($"XXXXXXXXXXXXXXXXX SMB2ConnectionSource.CheckOut Instance:{client.GetHashCode()} STATUS {status}");

                if (client.IsConnected && status == NTStatus.STATUS_SUCCESS)
                {
                    _count--;
                    Console.WriteLine($"SMB2ConnectionSource.CheckOut Instance:{client.GetHashCode()} Client:{client.Key} IsConnected:{client.IsConnected} EXISTING:true");
                    return client;
                }

                Console.WriteLine($"SMB2ConnectionSource.CheckOut - CLIENT WAS DOA");
            }

            client = createNewClient();
           Console.WriteLine($"SMB2ConnectionSource.CheckOut Instance:{client.GetHashCode()} Client:{client.Key} IsConnected:{client.IsConnected} EXISTING:false");
            _connections.Add(client);
            _count++;
            return client;

        }

        public void CheckIn(SMB2ConnectionClient client)
        {
            if (_count < _maxConnections && client.IsConnected)
            {

                _connections.Add(client);
                _count++;
            }
        }

        private SMB2ConnectionClient createNewClient()
        {
            return new SMB2ConnectionClient(_domain, _username, _password, _host, _share);
        }
    }
}
