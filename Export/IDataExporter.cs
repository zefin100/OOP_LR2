using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XmlProcessorApp.Models;

namespace XmlProcessorApp.Export
{
    /// <summary>
    /// Інтерфейс продукту для патерну Factory Method.
    /// Визначає операцію збереження відфільтрованих даних у конкретний формат (XML або HTML).
    /// </summary>
    public interface IDataExporter
    {
        string FormatName { get; }
        string DefaultExtension { get; }
        string MimeType { get; }

        Task WriteDataAsync(List<SoftwareItem> items, Stream destinationStream);
    }
}
