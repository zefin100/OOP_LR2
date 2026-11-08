using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmlProcessorApp.Models;

namespace XmlProcessorApp.Export
{
    /// <summary>
    /// Конкретний продукт: експортер у формат XML.
    /// Формує валідний XML-документ з 2-рівневою структурою (Department -> Software).
    /// </summary>
    public class XmlDataExporter : IDataExporter
    {
        public string FormatName => "XML";
        public string DefaultExtension => ".xml";
        public string MimeType => "application/xml";

        public async Task WriteDataAsync(List<SoftwareItem> items, Stream destinationStream)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("FacultySoftwareNetwork",
                    new XAttribute("exportedAt", System.DateTime.Now.ToString("s")),
                    new XAttribute("itemCount", items.Count),
                    from item in items
                    group item by item.Department into deptGroup
                    select new XElement("Department",
                        new XAttribute("name", deptGroup.Key),
                        new XAttribute("code", deptGroup.FirstOrDefault()?.DepartmentCode ?? ""),
                        from sw in deptGroup
                        select new XElement("Software",
                            new XAttribute("Name", sw.Name),
                            string.IsNullOrEmpty(sw.Version) ? null : new XAttribute("Version", sw.Version),
                            string.IsNullOrEmpty(sw.Category) ? null : new XAttribute("Category", sw.Category),
                            string.IsNullOrEmpty(sw.Author) ? null : new XAttribute("Author", sw.Author),
                            string.IsNullOrEmpty(sw.License) ? null : new XAttribute("License", sw.License),
                            new XElement("Description", sw.Description),
                            new XElement("DistributionLocation", sw.DistributionLocation),
                            string.IsNullOrEmpty(sw.Requirements) ? null : new XElement("Requirements", sw.Requirements),
                            string.IsNullOrEmpty(sw.MaintainedBy) ? null : new XElement("MaintainedBy", sw.MaintainedBy)
                        )
                    )
                )
            );

            using var memoryStream = new MemoryStream();
            doc.Save(memoryStream);
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(destinationStream);
        }
    }
}
