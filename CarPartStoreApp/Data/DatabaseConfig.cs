using Microsoft.Data.Sqlite;
using System.IO;

namespace CarPartStoreApp.Data
{
    /// <summary>
    /// Database configuration and connection management
    /// Supports both Turso (cloud) and local SQLite databases
    /// </summary>
    public static class DatabaseConfig
    {
        private const string DatabaseFileName = "CarPartStore.db";
        private static readonly string DatabasePath;
        private static bool? _useTurso;
        private static string? _tursoDatabaseUrl;
        private static string? _tursoAuthToken;
        private static string? _tursoHttpUrl;

        static DatabaseConfig()
        {
            // Store database in the user's app data folder for proper Windows app behavior
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CarPartStoreApp");

            // Create directory if it doesn't exist
            Directory.CreateDirectory(appDataPath);

            DatabasePath = Path.Combine(appDataPath, DatabaseFileName);

            // Load environment variables
            LoadEnvironmentVariables();
        }

        /// <summary>
        /// Gets the full path to the database file
        /// </summary>
        public static string GetDatabasePath()
        {
            return DatabasePath;
        }

        /// <summary>
        /// Checks if Turso is configured and should be used
        /// </summary>
        public static bool UseTurso()
        {
            if (!_useTurso.HasValue)
            {
                _useTurso = !string.IsNullOrEmpty(_tursoDatabaseUrl) &&
                             !string.IsNullOrEmpty(_tursoAuthToken);
            }
            return _useTurso.Value;
        }

        /// <summary>
        /// Gets the Turso database URL
        /// </summary>
        public static string? GetTursoDatabaseUrl() => _tursoDatabaseUrl;

        /// <summary>
        /// Gets the Turso auth token
        /// </summary>
        public static string? GetTursoAuthToken() => _tursoAuthToken;

        /// <summary>
        /// Gets Turso HTTP URL (for API calls)
        /// </summary>
        public static string? GetTursoHttpUrl() => _tursoHttpUrl;

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
        /// Creates and returns a connection for Turso database
        /// </summary>
        public static SqliteConnection CreateTursoConnection()
        {
            if (string.IsNullOrEmpty(_tursoDatabaseUrl))
            {
                throw new InvalidOperationException("Turso database URL is not configured");
            }

            // Construct raw Turso connection string to avoid SQLite parsing issues
            // Format: Data Source=libsql://url;Auth Token=token
            string finalConnectionString;
            if (!string.IsNullOrEmpty(_tursoAuthToken))
            {
                finalConnectionString = $"DataSource={_tursoDatabaseUrl};AuthToken={_tursoAuthToken}";
            }
            else
            {
                finalConnectionString = $"DataSource={_tursoDatabaseUrl}";
            }

            var connection = new SqliteConnection(finalConnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Checks if the database file exists
        /// </summary>
        public static bool DatabaseExists()
        {
            return File.Exists(DatabasePath);
        }

        /// <summary>
        /// Loads environment variables from .env file
        /// </summary>
        private static void LoadEnvironmentVariables()
        {
            // Try loading from .env file first
            LoadFromEnvFile();

            // Fallback to system environment variables
            _tursoDatabaseUrl = Environment.GetEnvironmentVariable("TURSO_DATABASE_URL");
            _tursoAuthToken = Environment.GetEnvironmentVariable("TURSO_AUTH_TOKEN");
            _tursoHttpUrl = Environment.GetEnvironmentVariable("TURSO_HTTP_URL");
        }

        /// <summary>
        /// Loads variables from .env file
        /// </summary>
        private static void LoadFromEnvFile()
        {
            var possibleLocations = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), ".env"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env"),
                Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.FullName ?? "", ".env")
            };

            foreach (var location in possibleLocations)
            {
                if (File.Exists(location))
                {
                    ParseEnvFile(location);
                    break;
                }
            }
        }

        /// <summary>
        /// Parses .env file and sets environment variables
        /// </summary>
        private static void ParseEnvFile(string filePath)
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                var parts = trimmedLine.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
    }
}
