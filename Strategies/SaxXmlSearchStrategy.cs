using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XmlProcessorApp.Logging;
using XmlProcessorApp.Models;

namespace XmlProcessorApp.Strategies
{
    /// <summary>
    /// Конкретна стратегія SAX (Simple API for XML) на основі потокового класу XmlReader.
    /// Працює за принципом прямого читання потоку «на льоту» без завантаження всього дерева в оперативну пам'ять.
    /// </summary>
    public class SaxXmlSearchStrategy : IXmlSearchStrategy
    {
        public string Name => "SAX API (XmlReader)";
        public string Technology => "SAX";
        public string Description => "Потоковий однопрохідний розбір через XmlReader (мінімальне використання пам'яті).";

        public List<SoftwareItem> Search(Stream xmlStream, SearchFilter filter)
        {
            if (xmlStream.CanSeek) xmlStream.Position = 0;

            var results = new List<SoftwareItem>();
            var settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
                DtdProcessing = DtdProcessing.Ignore
            };

            string currentDept = string.Empty;
            string currentDeptCode = string.Empty;

            using (var reader = XmlReader.Create(xmlStream, settings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name.Equals("Department", StringComparison.OrdinalIgnoreCase))
                        {
                            currentDept = reader.GetAttribute("name") ?? string.Empty;
                            currentDeptCode = reader.GetAttribute("code") ?? string.Empty;
                        }
                        else if (reader.Name.Equals("Software", StringComparison.OrdinalIgnoreCase))
                        {
                            var item = new SoftwareItem
                            {
                                Department = currentDept,
                                DepartmentCode = currentDeptCode,
                                Name = reader.GetAttribute("Name") ?? string.Empty,
                                Version = reader.GetAttribute("Version") ?? string.Empty,
                                Category = reader.GetAttribute("Category") ?? string.Empty,
                                Author = reader.GetAttribute("Author") ?? string.Empty,
                                License = reader.GetAttribute("License") ?? string.Empty
                            };

                            // Зчитуємо всі наявні атрибути вузла (різна кількість згідно з вимогою методички)
                            if (reader.HasAttributes)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    item.Attributes[reader.Name] = reader.Value;
                                }
                                reader.MoveToElement(); // повертаємося до самого елемента
                            }

                            // Якщо це порожній елемент <Software ... />, то дочірніх тегів немає
                            if (!reader.IsEmptyElement)
                            {
                                ReadChildElements(reader, item);
                            }

                            if (filter == null || filter.Matches(item))
                            {
                                results.Add(item);
                            }
                        }
                    }
                }
            }

            // Протоколювання через Singleton Logger
            AppLogger.Instance.Log(LogCategory.Filter,
                $"Стратегія: {Name}. Параметри: {filter}. Знайдено записів: {results.Count}");

            return results;
        }

        private static void ReadChildElements(XmlReader reader, SoftwareItem item)
        {
            int depth = reader.Depth;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth)
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    string elName = reader.Name;
                    string elText = reader.ReadElementContentAsString();

                    switch (elName.ToLowerInvariant())
                    {
                        case "description":
                            item.Description = elText;
                            break;
                        case "distributionlocation":
                            item.DistributionLocation = elText;
                            break;
                        case "requirements":
                            item.Requirements = elText;
                            break;
                        case "maintainedby":
                            item.MaintainedBy = elText;
                            break;
                    }
                }
            }
        }
    }
}
