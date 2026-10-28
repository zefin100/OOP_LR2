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

            string xpath = "//Department/Software";
            try
            {
                // Будуємо динамічний XPath-запит на основі вказаних фільтрів
                xpath = BuildXPath(filter);
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
            }
            catch (Exception ex)
            {
                // Безпечний fallback у разі нестандартних символів або складних виразів
                var allNodes = xmlDoc.SelectNodes("//Department/Software") ?? xmlDoc.GetElementsByTagName("Software");
                foreach (XmlNode node in allNodes)
                {
                    if (node is XmlElement element)
                    {
                        var item = ParseElement(element);
                        if (filter == null || filter.Matches(item))
                        {
                            results.Add(item);
                        }
                    }
                }

                AppLogger.Instance.Log(LogCategory.Filter,
                    $"Стратегія: {Name} (fallback: {ex.Message}). Параметри: {filter}. Знайдено записів: {results.Count}");
            }

            return results;
        }

        private static string FormatXPathLiteral(string val)
        {
            if (!val.Contains('\''))
            {
                return $"'{val}'";
            }
            if (!val.Contains('"'))
            {
                return $"\"{val}\"";
            }
            // Якщо рядок містить і ' і " -> concat('a', "'", 'b', '"', 'c')
            var parts = val.Split('\'');
            var quoted = new List<string>();
            for (int i = 0; i < parts.Length; i++)
            {
                quoted.Add($"'{parts[i]}'");
                if (i < parts.Length - 1)
                {
                    quoted.Add("\"'\"");
                }
            }
            return $"concat({string.Join(", ", quoted)})";
        }

        private static string BuildXPath(SearchFilter filter)
        {
            // Початковий шлях з підтримкою будь-якого регістру (Department / department)
            string deptPattern = "*[translate(local-name(), 'DEPARTMENT', 'department')='department']";
            string swPattern = "*[translate(local-name(), 'SOFTWARE', 'software')='software']";

            string xpath = $"//{deptPattern}";

            if (filter != null && !string.IsNullOrWhiteSpace(filter.Department) && filter.Department != "Всі кафедри")
            {
                xpath += $"[@*[translate(local-name(), 'NAME', 'name')='name']={FormatXPathLiteral(filter.Department)}]";
            }

            xpath += $"/{swPattern}";

            var predicates = new List<string>();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Category) && filter.Category != "Всі категорії")
                {
                    predicates.Add($"@*[translate(local-name(), 'CATEGORY', 'category')='category']={FormatXPathLiteral(filter.Category)}");
                }
                if (!string.IsNullOrWhiteSpace(filter.Author) && filter.Author != "Всі автори")
                {
                    predicates.Add($"@*[translate(local-name(), 'AUTHOR', 'author')='author']={FormatXPathLiteral(filter.Author)}");
                }
                if (!string.IsNullOrWhiteSpace(filter.License) && filter.License != "Всі ліцензії")
                {
                    predicates.Add($"@*[translate(local-name(), 'LICENSE', 'license')='license']={FormatXPathLiteral(filter.License)}");
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
                Name = GetAttr(element, "Name"),
                Version = GetAttr(element, "Version"),
                Category = GetAttr(element, "Category"),
                Author = GetAttr(element, "Author"),
                License = GetAttr(element, "License")
            };

            // Отримуємо назву батьківського вузла Department
            if (element.ParentNode is XmlElement deptEl && deptEl.Name.Equals("Department", StringComparison.OrdinalIgnoreCase))
            {
                item.Department = GetAttr(deptEl, "name");
                item.DepartmentCode = GetAttr(deptEl, "code");
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

        private static string GetAttr(XmlElement el, string name)
        {
            foreach (XmlAttribute attr in el.Attributes)
            {
                if (attr.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return attr.Value;
            }
            return string.Empty;
        }
    }
}
