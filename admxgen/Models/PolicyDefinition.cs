namespace admxgen.Models
{
    public class PolicyDefinition
    {
        public string CategoryId { get; set; }
        public string Category { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Class { get; set; }
        public string Type { get; set; }
        public string Explanation { get; set; }
        public string RegistryKey { get; set; }
        public string ValueName { get; set; }
        public string SupportedOnId { get; set; }
        public string SupportedOn { get; set; }
        public string Properties { get; set; }
    }
}
