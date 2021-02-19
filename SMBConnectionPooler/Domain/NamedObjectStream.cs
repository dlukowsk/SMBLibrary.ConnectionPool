using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SMBConnectionPooler.Domain
{
    public class NamedObjectStream : MemoryStream
    {
        public NamedObjectStream(byte[] buffer, string name, long totalFileLength) : this(buffer, name)
        {
            TotalFileLength = totalFileLength;
        }

        public NamedObjectStream(byte[] buffer, string name) : this(buffer)
        {
            Name = name;
        }

        public NamedObjectStream(byte[] buffer) : base(buffer)
        {
        }

        public NamedObjectStream()
        {
        }

        public string Name { get; set; }

        public long TotalFileLength { get; set; }

        public double RetrieveSpeedMs { get; set; }

        public double DecryptSpeedMs { get; set; }
    }
}
