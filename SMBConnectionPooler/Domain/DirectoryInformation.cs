using System.Runtime.Serialization;

namespace SMBConnectionPooler.Domain
{
    [DataContract]
    public class DirectoryInformation
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "fullPath")]
        public string FullPath { get; set; }

        [DataMember(Name = "exists")]
        public bool Exists { get; set; }
    }
}