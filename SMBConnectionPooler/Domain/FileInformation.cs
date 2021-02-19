using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SMBConnectionPooler.Domain
{
    [DataContract]
    public class FileInformation
    {
        [DataMember(Name = "directoryInfo")]
        public DirectoryInformation DirectoryInfo { get; set; }

        [DataMember(Name = "directoryName")]
        public string DirectoryName { get; set; }

        [DataMember(Name = "length")]
        public long Length { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "exists")]
        public bool Exists { get; set; }

        [DataMember(Name = "fullPath")]
        public string FullPath { get; set; }
    }
}
