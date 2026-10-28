using System;

namespace XmlProcessorApp.Logging
{
    public class LogEntry
    {
        public DateTime Timestamp { get; }
        public LogCategory Category { get; }
        public string Message { get; }

        public LogEntry(LogCategory category, string message)
        {
            Timestamp = DateTime.Now;
            Category = category;
            Message = message;
        }

        public override string ToString()
        {
            // Формат за зразком методички:
            // 21.09.2023 13:02:22 Фільтрація. Параметри: ...
            return $"{Timestamp:dd.MM.yyyy HH:mm:ss} {Category.ToDisplayString()}. {Message}";
        }
    }
}
