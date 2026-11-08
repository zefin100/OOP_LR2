using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using XmlProcessorApp.Export;
using XmlProcessorApp.Logging;
using XmlProcessorApp.Models;
using XmlProcessorApp.Services;
using XmlProcessorApp.Strategies;

namespace XmlProcessorApp
{
    public partial class MainPage : ContentPage
    {
        private readonly XmlSearchContext _searchContext;
        private readonly GoogleDriveManager _driveManager;

        private byte[]? _currentXmlBytes;
        private string _currentXmlFileName = "FacultySoftware.xml";
        private List<SoftwareItem> _allItems = new();
        private List<SoftwareItem> _filteredItems = new();
        private string _lastGeneratedHtml = string.Empty;
        private bool _isPopulatingFilters = false;

        public MainPage()
        {
            InitializeComponent();

            _searchContext = new XmlSearchContext(new LinqXmlSearchStrategy());
            _driveManager = new GoogleDriveManager();

            StrategyPicker.SelectedIndex = 0; // За замовчуванням LINQ to XML
            AppLogger.Instance.LogAdded += entry => Dispatcher.Dispatch(() =>
            {
                AppLogger.Instance.RecentLogs.Insert(0, entry);
                while (AppLogger.Instance.RecentLogs.Count > 200)
                {
                    AppLogger.Instance.RecentLogs.RemoveAt(AppLogger.Instance.RecentLogs.Count - 1);
                }
            });
            LogCollectionView.ItemsSource = AppLogger.Instance.RecentLogs;
            LogFilePathLabel.Text = $"Файл логу: {AppLogger.Instance.LogFilePath}";

            // Завантажуємо базовий XML-файл при старті
            LoadDefaultXmlFile();
        }

        private void LoadDefaultXmlFile()
        {
            try
            {
                string[] candidatePaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "FacultySoftware.xml"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Data", "FacultySoftware.xml"),
                    @"C:\My\Programming\OOP_LR2\Data\FacultySoftware.xml"
                };

                string? xmlPath = candidatePaths.FirstOrDefault(File.Exists);

                if (xmlPath != null)
                {
                    _currentXmlBytes = File.ReadAllBytes(xmlPath);
                    _currentXmlFileName = "FacultySoftware.xml";
                    ProcessLoadedXml();
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Помилка завантаження файлу за замовчуванням: {ex.Message}";
            }
        }

        private void ProcessLoadedXml()
        {
            if (_currentXmlBytes == null) return;

            // 1. Зчитуємо всі елементи поточною стратегією без фільтрів
            using var stream = new MemoryStream(_currentXmlBytes);
            _allItems = _searchContext.ExecuteSearch(stream, new SearchFilter());

            // 2. Виконуємо «Підгрузку даних» у випадаючі списки
            PopulateDynamicFilters(_allItems);

            // 3. Оновлюємо пошук
            ExecuteSearch();

            StatusLabel.Text = $"Завантажено файл: {_currentXmlFileName} ({_allItems.Count} програмних засобів)";
        }

        /// <summary>
        /// Реалізація вимоги методички: «підгрузка даних»
        /// При виборі файлу параметри пошуку автоматично наповнюються унікальними значеннями з самого XML.
        /// </summary>
        private void PopulateDynamicFilters(List<SoftwareItem> items)
        {
            if (items == null || items.Count == 0) return;

            try
            {
                _isPopulatingFilters = true;

                // Кафедри
                var departments = new List<string> { "Всі кафедри" };
                departments.AddRange(items.Select(i => i.Department).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct().OrderBy(d => d));
                DepartmentPicker.ItemsSource = departments;
                DepartmentPicker.SelectedIndex = 0;

                // Категорії / Вид ПЗ
                var categories = new List<string> { "Всі категорії" };
                categories.AddRange(items.Select(i => i.Category).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c));
                CategoryPicker.ItemsSource = categories;
                CategoryPicker.SelectedIndex = 0;

                // Автори / Розробники
                var authors = new List<string> { "Всі автори" };
                authors.AddRange(items.Select(i => i.Author).Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().OrderBy(a => a));
                AuthorPicker.ItemsSource = authors;
                AuthorPicker.SelectedIndex = 0;

                // Ліцензії / Умови використання
                var licenses = new List<string> { "Всі ліцензії" };
                licenses.AddRange(items.Select(i => i.License).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().OrderBy(l => l));
                LicensePicker.ItemsSource = licenses;
                LicensePicker.SelectedIndex = 0;
            }
            finally
            {
                _isPopulatingFilters = false;
            }
        }

        private void ExecuteSearch()
        {
            if (_currentXmlBytes == null)
            {
                DisplayAlert("Помилка", "Будь ласка, спочатку завантажте XML файл.", "OK");
                return;
            }

            // Якщо була відкрита панель HTML-звіту або логу - приховуємо їх, щоб користувач бачив оновлені картки ПЗ
            HtmlPreviewPanel.IsVisible = false;

            var filter = new SearchFilter
            {
                Keyword = KeywordEntry.Text,
                Department = DepartmentPicker.SelectedItem as string,
                Category = CategoryPicker.SelectedItem as string,
                Author = AuthorPicker.SelectedItem as string,
                License = LicensePicker.SelectedItem as string
            };

            try
            {
                using var stream = new MemoryStream(_currentXmlBytes);
                _filteredItems = _searchContext.ExecuteSearch(stream, filter);
                ResultsCollectionView.ItemsSource = null;
                ResultsCollectionView.ItemsSource = _filteredItems;

                CountLabel.Text = $"Знайдено: {_filteredItems.Count} із {_allItems.Count}";
            }
            catch (Exception ex)
            {
                DisplayAlert("Помилка пошуку", ex.Message, "OK");
            }
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (_isPopulatingFilters) return;
            // Автоматичний динамічний пошук при зміні будь-якого випадаючого списку
            ExecuteSearch();
        }

        private void OnKeywordChanged(object sender, TextChangedEventArgs e)
        {
            // Автоматичний пошук «на льоту» при введенні або очищенні тексту
            ExecuteSearch();
        }

        private void SearchButton_Clicked(object sender, EventArgs e)
        {
            ExecuteSearch();
        }

        /// <summary>
        /// Реалізація вимоги методички: кнопка Clear, яка очищує поле виводу і задані параметри пошуку.
        /// </summary>
        private void ClearButton_Clicked(object sender, EventArgs e)
        {
            KeywordEntry.Text = string.Empty;
            if (DepartmentPicker.ItemsSource?.Count > 0) DepartmentPicker.SelectedIndex = 0;
            if (CategoryPicker.ItemsSource?.Count > 0) CategoryPicker.SelectedIndex = 0;
            if (AuthorPicker.ItemsSource?.Count > 0) AuthorPicker.SelectedIndex = 0;
            if (LicensePicker.ItemsSource?.Count > 0) LicensePicker.SelectedIndex = 0;

            ExecuteSearch();

            AppLogger.Instance.Log(LogCategory.Filter, "Параметри пошуку очищено кнопкою Clear.");
        }

        private void StrategyPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = StrategyPicker.SelectedIndex;
            if (index >= 0 && index < XmlSearchContext.AvailableStrategies.Count)
            {
                _searchContext.CurrentStrategy = XmlSearchContext.AvailableStrategies[index];
                StatusLabel.Text = $"Активна стратегія: {_searchContext.CurrentStrategy.Name}";

                // Якщо файл вже завантажений, перераховуємо результати обраною стратегією
                if (_currentXmlBytes != null)
                {
                    ExecuteSearch();
                }
            }
        }

        private async void LoadLocalXml_Clicked(object sender, EventArgs e)
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Оберіть файл XML з програмним забезпеченням",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".xml" } },
                        { DevicePlatform.Android, new[] { "text/xml", "application/xml" } },
                        { DevicePlatform.iOS, new[] { "public.xml" } },
                        { DevicePlatform.MacCatalyst, new[] { "public.xml" } }
                    })
                };

                var result = await FilePicker.Default.PickAsync(options);
                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    _currentXmlBytes = ms.ToArray();
                    _currentXmlFileName = result.FileName;

                    ProcessLoadedXml();
                    await DisplayAlert("Успіх", $"Файл \"{result.FileName}\" успішно завантажено!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка відкриття файлу", ex.Message, "OK");
            }
        }

        private async void LoadDriveXml_Clicked(object sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = "Підключення до Google Drive...";
                var files = await _driveManager.ListXmlFilesAsync();

                if (files.Count == 0)
                {
                    await DisplayAlert("Google Drive", "На вашому Google Drive не знайдено файлів XML/HTML.", "OK");
                    return;
                }

                string[] fileNames = files.Select(f => f.Name).ToArray();
                string selectedFileName = await DisplayActionSheet("Оберіть файл з Google Drive", "Скасувати", null, fileNames);

                if (!string.IsNullOrEmpty(selectedFileName) && selectedFileName != "Скасувати")
                {
                    var selectedFile = files.First(f => f.Name == selectedFileName);
                    using var ms = new MemoryStream();
                    await _driveManager.DownloadFileAsync(selectedFile.Id, ms);
                    _currentXmlBytes = ms.ToArray();
                    _currentXmlFileName = selectedFile.Name;

                    ProcessLoadedXml();
                    await DisplayAlert("Успіх", $"Файл \"{selectedFileName}\" завантажено з Google Drive!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка Google Drive", ex.Message, "OK");
            }
        }

        private void TransformHtml_Clicked(object sender, EventArgs e)
        {
            if (_filteredItems.Count == 0)
            {
                DisplayAlert("Увага", "Немає відфільтрованих даних для трансформації.", "OK");
                return;
            }

            try
            {
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

                using var xslStream = File.OpenRead(xslPath);
                _lastGeneratedHtml = XmlTransformer.TransformItemsToHtml(_filteredItems, xslStream);

                HtmlWebView.Source = new HtmlWebViewSource { Html = _lastGeneratedHtml };
                HtmlPreviewPanel.IsVisible = true;
            }
            catch (Exception ex)
            {
                DisplayAlert("Помилка XSLT-трансформації", ex.Message, "OK");
            }
        }

        private async void OpenInBrowser_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_lastGeneratedHtml)) return;

            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), "FacultySoftwareReport.html");
                await File.WriteAllTextAsync(tempFile, _lastGeneratedHtml, Encoding.UTF8);
                await Launcher.Default.OpenAsync(new OpenFileRequest("Звіт HTML", new ReadOnlyFile(tempFile)));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка відкриття", ex.Message, "OK");
            }
        }

        private void CloseHtmlPreview_Clicked(object sender, EventArgs e)
        {
            HtmlPreviewPanel.IsVisible = false;
        }

        private async void ExportXmlDrive_Clicked(object sender, EventArgs e)
        {
            if (_filteredItems.Count == 0)
            {
                await DisplayAlert("Увага", "Немає відфільтрованих даних для збереження.", "OK");
                return;
            }

            try
            {
                // Застосування патерну Factory Method (Фабричний метод)
                GoogleDriveExportService exportService = new GoogleDriveXmlExportService();
                string fileName = $"FilteredSoftware_{DateTime.Now:yyyyMMdd_HHmmss}";

                string fileId = await exportService.ExportAsync(_filteredItems, fileName, _driveManager);
                await DisplayAlert("Збережено на Google Drive",
                    $"Відфільтрований XML успішно завантажено на ваш Google Drive!\n\n" +
                    $"• Файл: {fileName}.xml\n" +
                    $"• Збережено записів: {_filteredItems.Count}\n" +
                    $"• Хмарний ідентифікатор файлу (Google Drive File ID): {fileId}\n\n" +
                    $"Файл уже доступний у вашому хмарному сховищі Google Drive.",
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка збереження на Drive", ex.Message, "OK");
            }
        }

        private async void ExportHtmlDrive_Clicked(object sender, EventArgs e)
        {
            if (_filteredItems.Count == 0)
            {
                await DisplayAlert("Увага", "Немає відфільтрованих даних для збереження.", "OK");
                return;
            }

            try
            {
                // Застосування патерну Factory Method (Фабричний метод)
                GoogleDriveExportService exportService = new GoogleDriveHtmlExportService();
                string fileName = $"SoftwareReport_{DateTime.Now:yyyyMMdd_HHmmss}";

                string fileId = await exportService.ExportAsync(_filteredItems, fileName, _driveManager);
                await DisplayAlert("Збережено на Google Drive",
                    $"Згенерований HTML-звіт успішно завантажено на ваш Google Drive!\n\n" +
                    $"• Файл: {fileName}.html\n" +
                    $"• Включено записів: {_filteredItems.Count}\n" +
                    $"• Хмарний ідентифікатор файлу (Google Drive File ID): {fileId}\n\n" +
                    $"Ви можете відкрити його прямо в браузері через Google Drive.",
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Помилка збереження на Drive", ex.Message, "OK");
            }
        }

        private void ToggleLogView_Clicked(object sender, EventArgs e)
        {
            LogPanel.IsVisible = !LogPanel.IsVisible;
        }

        private void CloseLog_Clicked(object sender, EventArgs e)
        {
            LogPanel.IsVisible = false;
        }

        private void ClearLog_Clicked(object sender, EventArgs e)
        {
            AppLogger.Instance.ClearLog();
            DisplayAlert("Журнал", "Журнал подій успішно очищено.", "OK");
        }

        private void HelpButton_Clicked(object sender, EventArgs e)
        {
            DisplayAlert("Довідка про програму",
                "Лабораторна робота № 2* (Високий рівень)\n" +
                "Тема: Робота з файлами XML (Google Drive, Strategy, Factory Method, Singleton)\n" +
                "Варіант 9: \"Програмне забезпечення на мережі факультету\"\n\n" +
                "Розробник: Черненко Ігор Ігорович\n" +
                "Група: К-28\n" +
                "Факультет комп'ютерних наук та кібернетики КНУ\n\n" +
                "Патерни проєктування:\n" +
                "• Strategy — 3 алгоритми аналізу XML (SAX, DOM, LINQ to XML)\n" +
                "• Factory Method — експорт відфільтрованих даних у XML та HTML на Google Drive\n" +
                "• Singleton — потокобезпечне протоколювання подій програми (Logger)\n\n" +
                "GitHub: https://github.com/zefin100/OOP_LR2", "OK");
        }

        /// <summary>
        /// Реалізація вимоги методички: підтвердження при виході з програми
        /// («Чи дійсно ви хочете завершити роботу з програмою?» – «Так», «Ні»)
        /// </summary>
        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert(
                "Підтвердження виходу",
                "Чи дійсно ви хочете завершити роботу з програмою?",
                "Так",
                "Ні");

            if (answer)
            {
                Application.Current?.Quit();
            }
        }
    }
}
