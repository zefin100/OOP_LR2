using System;
using System.Collections.Generic;

namespace XmlProcessorApp.Models
{
    public class SoftwareItem
    {
        public string Department { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string DistributionLocation { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string MaintainedBy { get; set; } = string.Empty;

        // Зберігає всі сирі XML-атрибути вузла для повної гнучкості
        public Dictionary<string, string> Attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public override string ToString()
        {
            return $"{Name} (v{Version}) - [{Category}] - {Department}";
        }
    }
}
