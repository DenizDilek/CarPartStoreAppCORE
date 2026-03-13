namespace CarPartStoreApp.Localization
{
    /// <summary>
    /// Interface for localization service
    /// Provides language management and resource access
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets the current language code (e.g., "en" or "tr")
        /// </summary>
        string CurrentLanguage { get; }

        /// <summary>
        /// Gets the current language resources
        /// </summary>
        LanguageResources Resources { get; }

        /// <summary>
        /// Event raised when the language is changed
        /// </summary>
        event EventHandler? LanguageChanged;

        /// <summary>
        /// Changes the current language
        /// </summary>
        /// <param name="languageCode">Language code (e.g., "en", "tr")</param>
        void ChangeLanguage(string languageCode);

        /// <summary>
        /// Gets a localized string with optional parameters
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="args">Optional parameters for string formatting</param>
        /// <returns>Localized string</returns>
        string GetString(string key, params object[] args);
    }
}