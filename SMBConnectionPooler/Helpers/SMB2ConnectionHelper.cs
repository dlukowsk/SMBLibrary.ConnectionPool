using System;
using System.Collections.Generic;
using System.Text;

namespace SMBConnectionPooler.Helpers
{
    public class SMB2ConnectionHelper
    {
        public static string MakeKey(string host, string share)
        {
            return $"{host}_{share}";
        }
    }
}
