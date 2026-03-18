using CarPartStoreApp.Data;
using CarPartStoreApp.Models;
using System;

namespace CarPartStoreApp.Services
{
    /// <summary>
    /// Factory for creating data service instances
    /// Automatically chooses between Turso (cloud) and local SQLite based on configuration
    /// </summary>
    public static class DataServiceFactory
    {
        private static IDataService? _dataService;

        /// <summary>
        /// Gets the data service instance (singleton)
        /// Uses Turso if configured, otherwise falls back to local SQLite
        /// </summary>
        public static IDataService GetDataService()
        {
            if (_dataService == null)
            {
                _dataService = CreateDataService();
            }
            return _dataService;
        }

        /// <summary>
        /// Gets the data service instance for the specified database type
        /// Creates a new instance each time to allow dynamic switching
        /// </summary>
        /// <param name="databaseType">The type of database (Local or Cloud)</param>
        /// <returns>A new IDataService instance for the specified database type</returns>
        public static IDataService GetDataService(DatabaseType databaseType)
        {
            return CreateDataService(databaseType);
        }

        /// <summary>
        /// Creates a new data service instance
        /// Checks for Turso configuration and uses it if available
        /// </summary>
        private static IDataService CreateDataService()
        {
            try
            {
                if (DatabaseConfig.UseTurso())
                {
                    Console.WriteLine("🚀 Using Turso cloud database");
                    return new TursoDataService();
                }
                else
                {
                    Console.WriteLine("💾 Using local SQLite database");
                    return new SqliteDataService();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to initialize Turso database, falling back to local SQLite: {ex.Message}");
                return new SqliteDataService();
            }
        }

        /// <summary>
        /// Creates a new data service instance for the specified database type
        /// </summary>
        /// <param name="databaseType">The type of database to use</param>
        /// <returns>A new IDataService instance for the specified database type</returns>
        private static IDataService CreateDataService(DatabaseType databaseType)
        {
            try
            {
                if (databaseType == DatabaseType.Cloud)
                {
                    Console.WriteLine("🚀 Creating Turso cloud database connection");
                    return new TursoDataService();
                }
                else
                {
                    Console.WriteLine("💾 Creating local SQLite database connection");
                    return new SqliteDataService();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to create data service for {databaseType}: {ex.Message}");
                // Fall back to local SQLite if cloud fails
                if (databaseType == DatabaseType.Cloud)
                {
                    Console.WriteLine("⚠️ Falling back to local SQLite");
                    return new SqliteDataService();
                }
                throw;
            }
        }

        /// <summary>
        /// Forces recreation of the data service (useful for testing or configuration changes)
        /// </summary>
        public static void ResetDataService()
        {
            _dataService = null;
        }

        /// <summary>
        /// Gets the current database type being used
        /// </summary>
        public static string GetDatabaseType()
        {
            try
            {
                if (DatabaseConfig.UseTurso())
                {
                    return "Turso (Cloud)";
                }
                else
                {
                    return "Local SQLite";
                }
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}