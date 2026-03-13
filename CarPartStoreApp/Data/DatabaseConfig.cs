using Microsoft.Data.Sqlite;
using System.IO;

namespace CarPartStoreApp.Data
{
    /// <summary>
    /// Database configuration and connection management
    /// </summary>
    public static class DatabaseConfig
    {
        private const string DatabaseFileName = "CarPartStore.db";
        private static readonly string DatabasePath;

        static DatabaseConfig()
        {
            // Store database in the user's app data folder for proper Windows app behavior
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CarPartStoreApp");

            // Create directory if it doesn't exist
            Directory.CreateDirectory(appDataPath);

            DatabasePath = Path.Combine(appDataPath, DatabaseFileName);
        }

        /// <summary>
        /// Gets the full path to the database file
        /// </summary>
        public static string GetDatabasePath()
        {
            return DatabasePath;
        }

        /// <summary>
        /// Creates and returns a new SQLite connection
        /// </summary>
        public static SqliteConnection CreateConnection()
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = DatabasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                ForeignKeys = true, // Enable foreign key constraints
                Pooling = true // Enable connection pooling
            }.ToString();

            // Enable WAL mode for better concurrency
            var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Execute PRAGMA commands for WAL mode and synchronous mode
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
            command.ExecuteNonQuery();

            return connection;
        }

        /// <summary>
        /// Checks if the database file exists
        /// </summary>
        public static bool DatabaseExists()
        {
            return File.Exists(DatabasePath);
        }
    }
}