using System.Collections.Generic;

namespace admxgen.Models
{
    public class AdmxSettings
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Revision { get; set; }
        public string MinRequiredRevision { get; set; }
        public string SchemaVersion { get; set; }
        public TargetNamespace TargetNamespace { get; set; }
        public string FallbackCulture { get; set; }
        public List<string> SupersededPolicyFiles { get; set; }
    }

    public class TargetNamespace
    {
        public string Prefix { get; set; }
        public string Namespace { get; set; }
    }
}
