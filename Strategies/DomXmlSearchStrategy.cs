using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XmlProcessorApp.Logging;
using XmlProcessorApp.Models;

namespace XmlProcessorApp.Strategies
{
    /// <summary>
    /// Конкретна стратегія DOM (Document Object Model) на основі класу XmlDocument.
    /// Завантажує повне ієрархічне дерево документа в пам'ять і підтримує навігацію через вирази мови XPath.
    /// </summary>
    public class DomXmlSearchStrategy : IXmlSearchStrategy
    {
        public string Name => "DOM API (XmlDocument + XPath)";
        public string Technology => "DOM";
        public string Description => "Повна об'єктна модель документа в пам'яті з використанням запитів мови XPath.";

        public List<SoftwareItem> Search(Stream xmlStream, SearchFilter filter)
        {
            if (xmlStream.CanSeek) xmlStream.Position = 0;

            var results = new List<SoftwareItem>();
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlStream);

            // Будуємо динамічний XPath-запит на основі вказаних фільтрів
            string xpath = BuildXPath(filter);
            XmlNodeList? nodes = xmlDoc.SelectNodes(xpath);

            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    if (node is XmlElement element)
                    {
                        var item = ParseElement(element);
                        // Додаткова перевірка фільтру (наприклад, текстовий пошук у внутрішніх тегах)
                        if (filter == null || filter.Matches(item))
                        {
                            results.Add(item);
                        }
                    }
                }
            }

            // Протоколювання через Singleton Logger
            AppLogger.Instance.Log(LogCategory.Filter,
                $"Стратегія: {Name}. XPath: \"{xpath}\". Параметри: {filter}. Знайдено записів: {results.Count}");

            return results;
        }

        private static string BuildXPath(SearchFilter filter)
        {
            // Початковий шлях до всіх елементів Software
            string xpath = "//Department";

            if (filter != null && !string.IsNullOrWhiteSpace(filter.Department) && filter.Department != "Всі кафедри")
            {
                xpath += $"[@name='{filter.Department.Replace("'", "''")}']";
            }

            xpath += "/Software";

            var predicates = new List<string>();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Category) && filter.Category != "Всі категорії")
                {
                    predicates.Add($"@Category='{filter.Category.Replace("'", "''")}'");
                }
                if (!string.IsNullOrWhiteSpace(filter.Author) && filter.Author != "Всі автори")
                {
                    predicates.Add($"@Author='{filter.Author.Replace("'", "''")}'");
                }
                if (!string.IsNullOrWhiteSpace(filter.License) && filter.License != "Всі ліцензії")
                {
                    predicates.Add($"@License='{filter.License.Replace("'", "''")}'");
                }
            }

            if (predicates.Count > 0)
            {
                xpath += "[" + string.Join(" and ", predicates) + "]";
            }

            return xpath;
        }

        private static SoftwareItem ParseElement(XmlElement element)
        {
            var item = new SoftwareItem
            {
                Name = element.GetAttribute("Name"),
                Version = element.GetAttribute("Version"),
                Category = element.GetAttribute("Category"),
                Author = element.GetAttribute("Author"),
                License = element.GetAttribute("License")
            };

            // Отримуємо назву батьківського вузла Department
            if (element.ParentNode is XmlElement deptEl && deptEl.Name.Equals("Department", StringComparison.OrdinalIgnoreCase))
            {
                item.Department = deptEl.GetAttribute("name");
                item.DepartmentCode = deptEl.GetAttribute("code");
            }

            // Зчитуємо всі атрибути вузла
            foreach (XmlAttribute attr in element.Attributes)
            {
                item.Attributes[attr.Name] = attr.Value;
            }

            // Зчитуємо дочірні елементи
            foreach (XmlNode child in element.ChildNodes)
            {
                if (child is XmlElement childEl)
                {
                    switch (childEl.Name.ToLowerInvariant())
                    {
                        case "description":
                            item.Description = childEl.InnerText.Trim();
                            break;
                        case "distributionlocation":
                            item.DistributionLocation = childEl.InnerText.Trim();
                            break;
                        case "requirements":
                            item.Requirements = childEl.InnerText.Trim();
                            break;
                        case "maintainedby":
                            item.MaintainedBy = childEl.InnerText.Trim();
                            break;
                    }
                }
            }

            return item;
        }
    }
}
