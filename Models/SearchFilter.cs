using System;

namespace XmlProcessorApp.Models
{
    public class SearchFilter
    {
        public string? Keyword { get; set; }
        public string? Department { get; set; }
        public string? Category { get; set; }
        public string? Author { get; set; }
        public string? License { get; set; }

        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(Keyword) &&
            string.IsNullOrWhiteSpace(Department) &&
            string.IsNullOrWhiteSpace(Category) &&
            string.IsNullOrWhiteSpace(Author) &&
            string.IsNullOrWhiteSpace(License);

        public bool Matches(SoftwareItem item)
        {
            if (item == null) return false;

            // Фільтр за кафедрою
            if (!string.IsNullOrWhiteSpace(Department) &&
                !Department.Equals("Всі кафедри", StringComparison.OrdinalIgnoreCase) &&
                !item.Department.Equals(Department, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Фільтр за категорією/видом
            if (!string.IsNullOrWhiteSpace(Category) &&
                !Category.Equals("Всі категорії", StringComparison.OrdinalIgnoreCase) &&
                !item.Category.Equals(Category, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Фільтр за автором/розробником
            if (!string.IsNullOrWhiteSpace(Author) &&
                !Author.Equals("Всі автори", StringComparison.OrdinalIgnoreCase) &&
                !item.Author.Equals(Author, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Фільтр за ліцензією
            if (!string.IsNullOrWhiteSpace(License) &&
                !License.Equals("Всі ліцензії", StringComparison.OrdinalIgnoreCase) &&
                !item.License.Equals(License, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Пошук за ключовим словом (у назві, описі, розташуванні, вимогах)
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string kw = Keyword.Trim();
                bool matchesKeyword =
                    item.Name.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    item.Description.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    item.DistributionLocation.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    item.Requirements.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    item.Author.Contains(kw, StringComparison.OrdinalIgnoreCase);

                if (!matchesKeyword) return false;
            }

            return true;
        }

        public override string ToString()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(Keyword)) parts.Add($"Keyword='{Keyword}'");
            if (!string.IsNullOrWhiteSpace(Department) && Department != "Всі кафедри") parts.Add($"Department='{Department}'");
            if (!string.IsNullOrWhiteSpace(Category) && Category != "Всі категорії") parts.Add($"Category='{Category}'");
            if (!string.IsNullOrWhiteSpace(Author) && Author != "Всі автори") parts.Add($"Author='{Author}'");
            if (!string.IsNullOrWhiteSpace(License) && License != "Всі ліцензії") parts.Add($"License='{License}'");

            return parts.Count > 0 ? string.Join(", ", parts) : "Всі записи (без фільтрів)";
        }
    }
}
