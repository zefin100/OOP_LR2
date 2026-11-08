using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using XmlProcessorApp.Logging;
using XmlProcessorApp.Models;
using XmlProcessorApp.Services;

namespace XmlProcessorApp.Export
{
    /// <summary>
    /// Базовий клас творця (Creator) у патерні Factory Method (Фабричний метод).
    /// Містить загальний алгоритм підготовки файлу, запису через фабричний метод та завантаження на Google Drive.
    /// </summary>
    public abstract partial class GoogleDriveExportService
    {
        /// <summary>
        /// Фабричний метод: кожен підклас створює свій конкретний експортер (XmlDataExporter або HtmlDataExporter).
        /// </summary>
        protected abstract IDataExporter CreateExporter();

        /// <summary>
        /// Шаблонний метод експорту: створює відкритий потік у пам'яті, делегує запис конкретному
        /// експортеру, відправляє на Google Drive та фіксує подію в Singleton-логері.
        /// </summary>
        public async Task<string> ExportAsync(
            List<SoftwareItem> items, 
            string baseFileName, 
            GoogleDriveManager driveManager)
        {
            if (items == null || items.Count == 0)
            {
                throw new InvalidOperationException("Неможливо експортувати порожній список даних.");
            }

            // 1. Отримуємо конкретний експортер через Фабричний метод
            IDataExporter exporter = CreateExporter();

            string fullFileName = baseFileName.EndsWith(exporter.DefaultExtension, StringComparison.OrdinalIgnoreCase)
                ? baseFileName
                : baseFileName + exporter.DefaultExtension;

            // 2. Створюємо відкритий потік у пам'яті (код відкритого файлу)
            using (var memoryStream = new MemoryStream())
            {
                // 3. Записуємо дані у відповідному форматі
                await exporter.WriteDataAsync(items, memoryStream);
                memoryStream.Position = 0;

                // 4. Зберігаємо у Google Drive
                string fileId = await driveManager.UploadFileAsync(fullFileName, memoryStream, exporter.MimeType);

                // 5. Протоколюємо подію в Singleton Logger
                AppLogger.Instance.Log(LogCategory.Save,
                    $"Збережено відфільтрований фрагмент у Google Drive: файл \"{fullFileName}\" (ID: {fileId}), формат: {exporter.FormatName}");

                return fileId;
            }
        }
    }
}
