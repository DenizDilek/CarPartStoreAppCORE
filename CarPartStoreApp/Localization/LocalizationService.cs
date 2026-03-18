using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace CarPartStoreApp.Localization
{
    /// <summary>
    /// Simple service for managing application localization
    /// Reads JSON resource files from disk and provides dynamic language switching
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _languageResources;
        private readonly LanguageResources _resources;

        /// <summary>
        /// Gets current language code
        /// </summary>
        public string CurrentLanguage { get; private set; } = "en";

        /// <summary>
        /// Gets current language resources
        /// </summary>
        public LanguageResources Resources => _resources;

        /// <summary>
        /// Event raised when the language is changed
        /// </summary>
        public event EventHandler? LanguageChanged;

        /// <summary>
        /// Initializes a new instance of LocalizationService
        /// </summary>
        /// <param name="defaultLanguage">Default language code (default: "en")</param>
        public LocalizationService(string defaultLanguage = "en")
        {
            _languageResources = new Dictionary<string, Dictionary<string, string>>();
            _resources = new LanguageResources();
            LoadAllLanguageResources();
            ChangeLanguage(defaultLanguage);
        }

        /// <summary>
        /// Changes the current language
        /// </summary>
        /// <param name="languageCode">Language code (e.g., "en", "tr")</param>
        public void ChangeLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                languageCode = "en";
            }

            // If requested language doesn't exist, use English as fallback
            if (!_languageResources.ContainsKey(languageCode))
            {
                languageCode = "en";
            }

            CurrentLanguage = languageCode;
            _resources.SetResources(_languageResources[languageCode]);

            // Raise event to notify subscribers
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets a localized string with optional parameters
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="args">Optional parameters for string formatting</param>
        /// <returns>Localized string</returns>
        public string GetString(string key, params object[] args)
        {
            var value = _resources[key];

            if (args != null && args.Length > 0)
            {
                return string.Format(value, args);
            }

            return value;
        }

        /// <summary>
        /// Loads all language resource files from disk
        /// </summary>
        private void LoadAllLanguageResources()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            System.Diagnostics.Debug.WriteLine($"Base directory: {baseDirectory}");

            // Try to load from Resources folder first
            var resourcesPath = Path.Combine(baseDirectory, "Resources");
            System.Diagnostics.Debug.WriteLine($"Resources path: {resourcesPath}");
            if (Directory.Exists(resourcesPath))
            {
                LoadResourcesFromDirectory(resourcesPath);
            }

            // Fallback: try to load from the project's Resources folder
            var projectPath = Path.GetDirectoryName(AppContext.BaseDirectory);
            System.Diagnostics.Debug.WriteLine($"Project path: {projectPath}");
            if (projectPath != null)
            {
                var projectResourcesPath = Path.Combine(projectPath, "Resources");
                System.Diagnostics.Debug.WriteLine($"Project resources path: {projectResourcesPath}");
                if (Directory.Exists(projectResourcesPath))
                {
                    LoadResourcesFromDirectory(projectResourcesPath);
                }
            }

            // Ensure English is always available (fallback)
            if (!_languageResources.ContainsKey("en"))
            {
                _languageResources["en"] = new Dictionary<string, string>();
            }

            System.Diagnostics.Debug.WriteLine($"Loaded {_languageResources.Count} languages");

            // Show debug info in message box for visibility
            try
            {
                System.Windows.MessageBox.Show(
                    $"Resources loaded from: {baseDirectory}\n" +
                    $"Languages loaded: {_languageResources.Count}\n" +
                    $"Sample keys: {string.Join(", ", _languageResources.Values.FirstOrDefault()?.Keys.Take(3) ?? new List<string>())}",
                    "Debug - Resource Loading Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("MessageBox not available yet");
            }

        /// <summary>
        }

        /// <summary>
        /// Loads JSON resource files from a directory
        /// </summary>
        private void LoadResourcesFromDirectory(string directory)
        {
            System.Diagnostics.Debug.WriteLine($"Loading resources from directory: {directory}");
            var jsonFiles = Directory.GetFiles(directory, "Strings.*.json");
            System.Diagnostics.Debug.WriteLine($"Found {jsonFiles.Length} JSON files");

            foreach (var filePath in jsonFiles)
            {
                try
                {
                    // Extract language code from file name (e.g., "Strings.en.json" -> "en")
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var languageCode = fileName.Replace("Strings.", "");

                    System.Diagnostics.Debug.WriteLine($"Loading file: {filePath} as language: {languageCode}");

                    var json = File.ReadAllText(filePath);
                    var resources = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                    if (resources != null)
                    {
                        // Flatten the nested dictionary structure
                        var flatResources = FlattenDictionary(resources);
                        _languageResources[languageCode] = flatResources;
                        System.Diagnostics.Debug.WriteLine($"Loaded language '{languageCode}' with {flatResources.Count} resources");
                        System.Diagnostics.Debug.WriteLine($"Sample keys: {string.Join(", ", flatResources.Keys.Take(5))}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load language resource '{filePath}': {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Flattens a nested dictionary into dot-separated keys
        /// </summary>
        /// <param name="dictionary">Nested dictionary</param>
        /// <param name="prefix">Key prefix for recursion</param>
        /// <returns>Flattened dictionary</returns>
        private Dictionary<string, string> FlattenDictionary(Dictionary<string, JsonElement> dictionary, string prefix = "")
        {
            var result = new Dictionary<string, string>();

            foreach (var kvp in dictionary)
            {
                var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

                if (kvp.Value.ValueKind == JsonValueKind.Object)
                {
                    // Recursively flatten nested objects
                    var nestedDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(kvp.Value.GetRawText());
                    if (nestedDict != null)
                    {
                        var nestedResult = FlattenDictionary(nestedDict, key);
                        foreach (var nestedKvp in nestedResult)
                        {
                            result[nestedKvp.Key] = nestedKvp.Value;
                        }
                    }
                }
                else
                {
                    // Add string value
                    result[key] = kvp.Value.ToString();
                }
            }

            return result;
        }
    }
}
