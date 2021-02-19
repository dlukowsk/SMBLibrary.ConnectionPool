using System;
using System.Collections.Generic;
using System.Text;

namespace SMBConnectionPooler.Domain
{
    public class NamedObjectBytes
    {
        /// <summary>
        /// The total length of the bytes in the entire file, not just the range captured.
        /// </summary>
        public long? TotalFileLength { get; set; }

        /// <summary>
        /// The byes of the range requested or the entire file.
        /// </summary>
        public byte[] FileBytes { get; set; }

        public double RetrieveSpeedMs { get; set; }

        public double DecryptSpeedMe { get; set; }
    }
}
