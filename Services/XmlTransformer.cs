using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using XmlProcessorApp.Logging;
using XmlProcessorApp.Models;

namespace XmlProcessorApp.Services
{
    /// <summary>
    /// Сервіс для XSLT-трансформації XML у формат HTML за допомогою XslCompiledTransform.
    /// Підтримує трансформацію як повного XML-файлу, так і відфільтрованого фрагмента.
    /// </summary>
    public class XmlTransformer
    {
        public static string TransformToHtml(Stream xmlStream, Stream xslStream)
        {
            if (xmlStream.CanSeek) xmlStream.Position = 0;
            if (xslStream.CanSeek) xslStream.Position = 0;

            var xslt = new XslCompiledTransform();
            using (var xslReader = XmlReader.Create(xslStream))
            {
                xslt.Load(xslReader);
            }

            var outputSb = new StringBuilder();
            using (var xmlReader = XmlReader.Create(xmlStream))
            using (var textWriter = new StringWriter(outputSb))
            using (var htmlWriter = XmlWriter.Create(textWriter, xslt.OutputSettings))
            {
                xslt.Transform(xmlReader, htmlWriter);
            }

            string html = outputSb.ToString();

            // Протоколювання через Singleton Logger
            AppLogger.Instance.Log(LogCategory.Transform,
                $"Трансформація XSLT успішно виконана. Розмір згенерованого HTML: {html.Length} байт");

            return html;
        }

        public static string TransformItemsToHtml(List<SoftwareItem> items, Stream xslStream)
        {
            // Перетворюємо відфільтрований список у валідний XML-потік для XSLT
            var doc = new XDocument(
                new XElement("FacultySoftwareNetwork",
                    new XAttribute("university", "Київський національний університет імені Тараса Шевченка"),
                    new XAttribute("faculty", "Факультет комп'ютерних наук та кібернетики"),
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

            using var memStream = new MemoryStream();
            doc.Save(memStream);
            memStream.Position = 0;

            return TransformToHtml(memStream, xslStream);
        }
    }
}
