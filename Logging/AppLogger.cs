using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace XmlProcessorApp.Logging
{
    /// <summary>
    /// Патерн Singleton (Одинак) для централізованого протоколювання подій програми.
    /// Забезпечує єдину точку запису логів на локальний диск з підтримкою 3 рівнів важливості.
    /// Повністю відокремлений від UI-фреймворку (принцип Single Responsibility).
    /// </summary>
    public sealed class AppLogger
    {
        private static readonly Lazy<AppLogger> _instance = new(() => new AppLogger());
        private readonly object _lock = new();
        private readonly string _logFilePath;

        // In-memory колекція для відображення журналу подій в UI
        public ObservableCollection<LogEntry> RecentLogs { get; } = new();

        // Подія для сповіщення UI про новий запис (Observer)
        public event Action<LogEntry>? LogAdded;

        public static AppLogger Instance => _instance.Value;

        private AppLogger()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folder = Path.Combine(appData, "XmlProcessorApp");
            Directory.CreateDirectory(folder);
            _logFilePath = Path.Combine(folder, "event_log.txt");
        }

        public string LogFilePath => _logFilePath;

        public void Log(LogCategory category, string message)
        {
            var entry = new LogEntry(category, message);
            string line = entry.ToString();

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, line + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // Ignore disk write errors to prevent application crash
                }
            }

            try
            {
                LogAdded?.Invoke(entry);
            }
            catch
            {
                // Ignore listener exceptions
            }
        }

        public string ReadAllLogs()
        {
            lock (_lock)
            {
                if (File.Exists(_logFilePath))
                {
                    return File.ReadAllText(_logFilePath, Encoding.UTF8);
                }
                return "Журнал подій порожній.";
            }
        }

        public void ClearLog()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_logFilePath))
                    {
                        File.WriteAllText(_logFilePath, string.Empty);
                    }
                }
                catch
                {
                }
            }
            RecentLogs.Clear();
        }
    }
}
