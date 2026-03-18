using System;
using System.IO;
using System.Text.Json;
using CarPartStoreApp.Models;

namespace CarPartStoreApp.Data
{
    /// <summary>
    /// Cloudinary image storage configuration
    /// </summary>
    public class CloudinarySettings
    {
        /// <summary>
        /// Cloudinary cloud name
        /// </summary>
        public string CloudName { get; set; } = string.Empty;

        /// <summary>
        /// Cloudinary API key
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Cloudinary API secret
        /// </summary>
        public string ApiSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets whether Cloudinary is properly configured
        /// </summary>
        public bool IsConfigured =>
            !string.IsNullOrEmpty(CloudName) &&
            !string.IsNullOrEmpty(ApiKey) &&
            !string.IsNullOrEmpty(ApiSecret);
    }

    /// <summary>
    /// Application settings manager
    /// Persists user preferences including language selection and database type
    /// </summary>
    public class AppSettings
    {
        private const string SettingsFileName = "settings.json";
        private static readonly string SettingsPath;

        /// <summary>
        /// Gets or sets the current language code (e.g., "en", "tr")
        /// </summary>
        public string CurrentLanguage { get; set; } = "en";

        /// <summary>
        /// Gets or sets the selected database type (Local or Cloud)
        /// </summary>
        public DatabaseType SelectedDatabase { get; set; } = DatabaseType.Local;

        /// <summary>
        /// Gets or sets the Cloudinary image storage settings
        /// </summary>
        public CloudinarySettings Cloudinary { get; set; } = new CloudinarySettings();

        /// <summary>
        /// Gets or sets the last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        /// <summary>
        /// Static constructor to initialize the settings path
        /// </summary>
        static AppSettings()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CarPartStoreApp");
            Directory.CreateDirectory(appDataPath);
            SettingsPath = Path.Combine(appDataPath, SettingsFileName);
        }

        /// <summary>
        /// Saves the current settings to file
        /// </summary>
        public void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SettingsPath, json);
            LastUpdated = DateTime.Now;
        }

        /// <summary>
        /// Loads settings from file or returns default settings if file doesn't exist
        /// </summary>
        /// <returns>AppSettings instance</returns>
        public static AppSettings Load()
        {
            if (!File.Exists(SettingsPath))
            {
                return new AppSettings();
            }

            try
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch (Exception)
            {
                // If there's an error loading settings, return default settings
                return new AppSettings();
            }
        }
    }
}