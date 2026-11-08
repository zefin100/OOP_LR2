using System;
using System.Collections.Generic;
using System.IO;
using XmlProcessorApp.Models;

namespace XmlProcessorApp.Strategies
{
    /// <summary>
    /// Контекст патерну Strategy (Стратегія).
    /// Зберігає посилання на поточну стратегію пошуку та дозволяє змінювати її під час роботи програми.
    /// </summary>
    public class XmlSearchContext
    {
        public IXmlSearchStrategy CurrentStrategy { get; set; }

        public static readonly IReadOnlyList<IXmlSearchStrategy> AvailableStrategies = new List<IXmlSearchStrategy>
        {
            new LinqXmlSearchStrategy(),
            new DomXmlSearchStrategy(),
            new SaxXmlSearchStrategy()
        };

        public XmlSearchContext(IXmlSearchStrategy? initialStrategy = null)
        {
            CurrentStrategy = initialStrategy ?? AvailableStrategies[0];
        }

        public List<SoftwareItem> ExecuteSearch(Stream xmlStream, SearchFilter filter)
        {
            if (CurrentStrategy == null)
            {
                throw new InvalidOperationException("Стратегію пошуку не встановлено.");
            }
            return CurrentStrategy.Search(xmlStream, filter);
        }
    }
}
