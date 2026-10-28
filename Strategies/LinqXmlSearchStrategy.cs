using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using XmlProcessorApp.Logging;
using XmlProcessorApp.Models;

namespace XmlProcessorApp.Strategies
{
    /// <summary>
    /// Конкретна стратегія LINQ to XML на основі класів XDocument та XElement.
    /// Забезпечує високу виразність завдяки декларативним запитам LINQ, строгій типізації та оптимізованій роботі з пам'яттю.
    /// </summary>
    public class LinqXmlSearchStrategy : IXmlSearchStrategy
    {
        public string Name => "LINQ to XML (XDocument)";
        public string Technology => "LINQ to XML";
        public string Description => "Декларативні запити LINQ to XML із використанням проекцій та лямбда-виразів.";

        public List<SoftwareItem> Search(Stream xmlStream, SearchFilter filter)
        {
            if (xmlStream.CanSeek) xmlStream.Position = 0;

            var doc = XDocument.Load(xmlStream);

            // Формуємо декларативний запит за зразком з лекцій
            var query = from dept in doc.Descendants()
                        where dept.Name.LocalName.Equals("Department", StringComparison.OrdinalIgnoreCase)
                        let deptName = (string?)dept.Attribute("name") ?? (string?)dept.Attribute("Name") ?? string.Empty
                        let deptCode = (string?)dept.Attribute("code") ?? (string?)dept.Attribute("Code") ?? string.Empty
                        from sw in dept.Elements()
                        where sw.Name.LocalName.Equals("Software", StringComparison.OrdinalIgnoreCase)
                        select new SoftwareItem
                        {
                            Department = deptName,
                            DepartmentCode = deptCode,
                            Name = (string?)sw.Attribute("Name") ?? (string?)sw.Attribute("name") ?? string.Empty,
                            Version = (string?)sw.Attribute("Version") ?? (string?)sw.Attribute("version") ?? string.Empty,
                            Category = (string?)sw.Attribute("Category") ?? (string?)sw.Attribute("category") ?? string.Empty,
                            Author = (string?)sw.Attribute("Author") ?? (string?)sw.Attribute("author") ?? string.Empty,
                            License = (string?)sw.Attribute("License") ?? (string?)sw.Attribute("license") ?? string.Empty,
                            Description = sw.Elements().FirstOrDefault(e => e.Name.LocalName.Equals("Description", StringComparison.OrdinalIgnoreCase))?.Value.Trim() ?? string.Empty,
                            DistributionLocation = sw.Elements().FirstOrDefault(e => e.Name.LocalName.Equals("DistributionLocation", StringComparison.OrdinalIgnoreCase))?.Value.Trim() ?? string.Empty,
                            Requirements = sw.Elements().FirstOrDefault(e => e.Name.LocalName.Equals("Requirements", StringComparison.OrdinalIgnoreCase))?.Value.Trim() ?? string.Empty,
                            MaintainedBy = sw.Elements().FirstOrDefault(e => e.Name.LocalName.Equals("MaintainedBy", StringComparison.OrdinalIgnoreCase))?.Value.Trim() ?? string.Empty,
                            Attributes = sw.Attributes().ToDictionary(
                                a => a.Name.LocalName,
                                a => a.Value,
                                StringComparer.OrdinalIgnoreCase)
                        };

            // Фільтрація за критеріями
            if (filter != null && !filter.IsEmpty)
            {
                query = query.Where(item => filter.Matches(item));
            }

            var results = query.ToList();

            // Протоколювання через Singleton Logger
            AppLogger.Instance.Log(LogCategory.Filter,
                $"Стратегія: {Name}. Параметри: {filter}. Знайдено записів: {results.Count}");

            return results;
        }
    }
}
