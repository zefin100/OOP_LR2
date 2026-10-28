using System.IO;

namespace XmlProcessorApp.Export
{
    /// <summary>
    /// Конкретний творець (Concrete Creator) для експорту у формат XML.
    /// Перевизначає фабричний метод CreateExporter(), повертаючи XmlDataExporter.
    /// </summary>
    public class GoogleDriveXmlExportService : GoogleDriveExportService
    {
        protected override IDataExporter CreateExporter()
        {
            return new XmlDataExporter();
        }
    }

    /// <summary>
    /// Конкретний творець (Concrete Creator) для експорту у формат HTML.
    /// Перевизначає фабричний метод CreateExporter(), повертаючи HtmlDataExporter.
    /// </summary>
    public class GoogleDriveHtmlExportService : GoogleDriveExportService
    {
        private readonly Stream? _customXslStream;

        public GoogleDriveHtmlExportService(Stream? customXslStream = null)
        {
            _customXslStream = customXslStream;
        }

        protected override IDataExporter CreateExporter()
        {
            return new HtmlDataExporter(_customXslStream);
        }
    }
}
