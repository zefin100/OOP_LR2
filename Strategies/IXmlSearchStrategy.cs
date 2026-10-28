using System.Collections.Generic;
using System.IO;
using XmlProcessorApp.Models;

namespace XmlProcessorApp.Strategies
{
    /// <summary>
    /// Інтерфейс патерну Strategy (Стратегія) для пошуку та розбору XML-документа.
    /// Забезпечує взаємозамінність трьох алгоритмів: SAX (XmlReader), DOM (XmlDocument) та LINQ to XML (XDocument).
    /// </summary>
    public interface IXmlSearchStrategy
    {
        string Name { get; }
        string Technology { get; }
        string Description { get; }

        List<SoftwareItem> Search(Stream xmlStream, SearchFilter filter);
    }
}
