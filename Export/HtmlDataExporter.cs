using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using XmlProcessorApp.Models;
using XmlProcessorApp.Services;

namespace XmlProcessorApp.Export
{
    /// <summary>
    /// Конкретний продукт: експортер у формат HTML.
    /// Перетворює відфільтровані дані на повноцінний стилізований HTML-звіт через XSLT.
    /// </summary>
    public class HtmlDataExporter : IDataExporter
    {
        private readonly Stream? _xslStream;

        public string FormatName => "HTML";
        public string DefaultExtension => ".html";
        public string MimeType => "text/html";

        public HtmlDataExporter(Stream? xslStream = null)
        {
            _xslStream = xslStream;
        }

        public async Task WriteDataAsync(List<SoftwareItem> items, Stream destinationStream)
        {
            Stream xslToUse;
            bool disposeXsl = false;

            if (_xslStream != null && _xslStream.CanRead)
            {
                xslToUse = _xslStream;
            }
            else
            {
                // Пошук файлу стилів у кількох можливих розташуваннях
                string[] candidatePaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SoftwareReport.xsl"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Data", "SoftwareReport.xsl"),
                    @"C:\My\Programming\OOP_LR2\Data\SoftwareReport.xsl"
                };

                string? xslPath = candidatePaths.FirstOrDefault(File.Exists);
                if (xslPath == null)
                {
                    throw new FileNotFoundException("Файл стилів XSLT 'SoftwareReport.xsl' не знайдено.");
                }

                xslToUse = File.OpenRead(xslPath);
                disposeXsl = true;
            }

            try
            {
                string htmlContent = XmlTransformer.TransformItemsToHtml(items, xslToUse);
                byte[] bytes = Encoding.UTF8.GetBytes(htmlContent);
                await destinationStream.WriteAsync(bytes, 0, bytes.Length);
            }
            finally
            {
                if (disposeXsl) xslToUse.Dispose();
            }
        }
    }
}
