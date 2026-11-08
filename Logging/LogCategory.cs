namespace XmlProcessorApp.Logging
{
    public enum LogCategory
    {
        Filter,     // Фільтрація
        Transform,  // Трансформація
        Save        // Збереження
    }

    public static class LogCategoryExtensions
    {
        public static string ToDisplayString(this LogCategory category)
        {
            return category switch
            {
                LogCategory.Filter => "Фільтрація",
                LogCategory.Transform => "Трансформація",
                LogCategory.Save => "Збереження",
                _ => category.ToString()
            };
        }
    }
}
