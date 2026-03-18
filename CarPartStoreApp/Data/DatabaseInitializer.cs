using CarPartStoreApp.Services;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace CarPartStoreApp.Data
{
    /// <summary>
    /// Database initialization helper
    /// Creates and initializes the SQLite database with required tables
    /// </summary>
    public class DatabaseInitializer
    {
        /// <summary>
        /// Initializes the SQLite database with required tables
        /// </summary>
        public static void Initialize()
        {
            // Use Turso initialization if enabled
            if (DatabaseConfig.UseTurso())
            {
                InitializeTursoDatabase();
            }
            else
            {
                // Create tables if they don't exist
                CreateTables();

                // Seed initial data if database is new
                if (!DatabaseConfig.DatabaseExists())
                {
                    SeedData();
                }
                else
                {
                    // Check if categories exist, if not seed them
                    EnsureCategoriesExist();
                }
            }
        }

        /// <summary>
        /// Initializes Turso cloud database schema
        /// </summary>
        private static void InitializeTursoDatabase()
        {
            try
            {
                var tursoService = ServiceContainer.TursoDataService;
                if (tursoService == null)
                {
                    return;
                }
                // Console.WriteLine("Initializing Turso database schema...");
                tursoService.InitializeDatabaseSchemaAsync().GetAwaiter().GetResult();
                // Console.WriteLine("Turso database initialized successfully!");
            }
            catch
            {
                // Silently ignore errors during initialization
            }
        }

        /// <summary>
        /// Creates database schema if it doesn't exist
        /// </summary>
        private static void CreateTables()
        {
            using var connection = DatabaseConfig.CreateConnection();

            // Create Categories table
            var createCategoriesTable = @"
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    ParentCategoryId INTEGER,
                    DisplayOrder INTEGER DEFAULT 0,
                    CreatedDate TEXT NOT NULL,
                    FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id)
                )";
            ExecuteNonQuery(connection, createCategoriesTable);

            // Create Parts table
            var createPartsTable = @"
                CREATE TABLE IF NOT EXISTS Parts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PartNumber TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    CategoryId INTEGER,
                    CostPrice REAL NOT NULL DEFAULT 0,
                    RetailPrice REAL NOT NULL DEFAULT 0,
                    StockQuantity INTEGER NOT NULL DEFAULT 0,
                    Location TEXT,
                    Supplier TEXT,
                    ImagePath TEXT,
                    Model TEXT,
                    ReleaseYear INTEGER,
                    CreatedDate TEXT NOT NULL,
                    LastUpdated TEXT,
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                )";
            ExecuteNonQuery(connection, createPartsTable);

            // Add ImagePath column if it doesn't exist (for existing databases)
            AddColumnIfNotExists(connection, "Parts", "ImagePath", "TEXT");

            // Add Model column if it doesn't exist (for existing databases)
            AddColumnIfNotExists(connection, "Parts", "Model", "TEXT");

            // Migrate ReleaseDate TEXT to ReleaseYear INTEGER
            // First add the new ReleaseYear column
            if (!ColumnExists(connection, "Parts", "ReleaseYear"))
            {
                AddColumnIfNotExists(connection, "Parts", "ReleaseYear", "INTEGER");

                // Migrate data from ReleaseDate to ReleaseYear (extract year from date string)
                var migrationSql = @"
                    UPDATE Parts
                    SET ReleaseYear = CAST(substr(COALESCE(ReleaseDate, '0000'), 1, 4) AS INTEGER)
                    WHERE ReleaseYear IS NULL AND ReleaseDate IS NOT NULL
                ";
                ExecuteNonQuery(connection, migrationSql);
                // Console.WriteLine("[SQLITE MIGRATION] Migrated data from ReleaseDate to ReleaseYear");
            }

            // Note: We keep the old ReleaseDate column for backward compatibility
            // The SqliteDataService.MapReaderToCarPart method handles both schemas

            // Create indexes for better performance
            CreateIndexes(connection);
        }

        /// <summary>
        /// Creates database indexes for better query performance
        /// </summary>
        private static void CreateIndexes(SqliteConnection connection)
        {
            // Index on PartNumber for fast lookups
            ExecuteNonQuery(connection, @"
                CREATE INDEX IF NOT EXISTS idx_parts_partnumber ON Parts(PartNumber)
            ");

            // Index on CategoryId for fast category filtering
            ExecuteNonQuery(connection, @"
                CREATE INDEX IF NOT EXISTS idx_parts_category ON Parts(CategoryId)
            ");

            // Index on Name for search functionality
            ExecuteNonQuery(connection, @"
                CREATE INDEX IF NOT EXISTS idx_parts_name ON Parts(Name)
            ");

            // Index on ParentCategoryId for category hierarchy
            ExecuteNonQuery(connection, @"
                CREATE INDEX IF NOT EXISTS idx_categories_parent ON Categories(ParentCategoryId)
            ");
        }

        /// <summary>
        /// Seeds the database with initial data if needed
        /// </summary>
        private static void SeedData()
        {
            using var connection = DatabaseConfig.CreateConnection();

            // Create default categories
            var seedCategories = @"
                INSERT OR IGNORE INTO Categories (Name, Description, ParentCategoryId, DisplayOrder, CreatedDate)
                VALUES
                    ('Engine', 'Engine components and parts', NULL, 1, datetime('now')),
                    ('Transmission', 'Transmission and drivetrain', NULL, 2, datetime('now')),
                    ('Brakes', 'Brake system components', NULL, 3, datetime('now')),
                    ('Suspension', 'Suspension and steering', NULL, 4, datetime('now')),
                    ('Electrical', 'Electrical system and lighting', NULL, 5, datetime('now')),
                    ('Body & Exterior', 'Body panels and exterior parts', NULL, 6, datetime('now')),
                    ('Interior', 'Interior components and accessories', NULL, 7, datetime('now')),
                    ('Filters', 'Oil, air, and fuel filters', NULL, 8, datetime('now')),
                    ('Belts & Hoses', 'Timing belts, serpentine belts, and hoses', NULL, 9, datetime('now')),
                    ('Miscellaneous', 'Other parts and accessories', NULL, 10, datetime('now'))
            ";
            ExecuteNonQuery(connection, seedCategories);

            // Create some sample parts
            var seedParts = @"
                INSERT OR IGNORE INTO Parts (PartNumber, Name, Description, CategoryId, CostPrice, RetailPrice, StockQuantity, Location, Supplier, CreatedDate)
                VALUES
                    ('ENG-001', 'Oil Filter', 'Standard oil filter for most engines', 8, 5.99, 12.99, 50, 'Aisle 1, Shelf 2', 'AutoParts Inc', datetime('now')),
                    ('ENG-002', 'Air Filter', 'High-flow air filter', 8, 8.99, 18.99, 35, 'Aisle 1, Shelf 3', 'AutoParts Inc', datetime('now')),
                    ('TRN-001', 'Transmission Gear', 2, 250.00, 500.00, 8, 'Aisle 2, Shelf 1', 'TransCo', datetime('now')),
                    ('BRK-001', 'Brake Pad Set', 4, 89.99, 120.00, 50, 'Aisle 2, Shelf 1', 'BrakeMaster', datetime('now')),
                    ('FUE-001', 'Fuel Pump Assembly', 5, 300.00, 450.00, 12, 'Aisle 3, Shelf 4', 'SuspensionPro', datetime('now')),
                    ('IGN-001', 'Spark Plug Kit', 7, 25.00, 35.00, 150, 'Aisle 4, Shelf 2', 'ElectroParts', datetime('now')),
                    ('COO-001', 'Radiator Assembly', 8, 125.00, 200.00, 5, 'Aisle 4, Shelf 1', 'RadiatorsRUs', datetime('now')),
                    ('ELE-001', 'Alternator Assembly', 9, 185.00, 350.00, 20, 'Aisle 4, Shelf 2', 'AlternatorsIntl', datetime('now')),
                    ('BOI-001', 'Seat Cushion Set', 10, 75.00, 120.00, 40, 'Aisle 4, Shelf 2', 'SeatMasters', datetime('now'));
            ";
            ExecuteNonQuery(connection, seedParts);
        }

        /// <summary>
        /// Ensures that categories exist in the database
        /// </summary>
        private static void EnsureCategoriesExist()
        {
            using var connection = DatabaseConfig.CreateConnection();

            // Check if categories exist
            using var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Categories";
            var count = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (count == 0)
            {
                // Seed categories
                var seedCategories = @"
                    INSERT INTO Categories (Name, Description, ParentCategoryId, DisplayOrder, CreatedDate)
                    VALUES
                        ('Engine', 'Engine components and parts', NULL, 1, datetime('now')),
                        ('Transmission', 'Transmission and drivetrain', NULL, 2, datetime('now')),
                        ('Brakes', 'Brake system components', NULL, 3, datetime('now')),
                        ('Suspension', 'Suspension and steering', NULL, 4, datetime('now')),
                        ('Electrical', 'Electrical system and lighting', NULL, 5, datetime('now')),
                        ('Body & Exterior', 'Body panels and exterior parts', NULL, 6, datetime('now')),
                        ('Interior', 'Interior components and accessories', NULL, 7, datetime('now')),
                        ('Filters', 'Oil, air, and fuel filters', NULL, 8, datetime('now')),
                        ('Belts & Hoses', 'Timing belts, serpentine belts, and hoses', NULL, 9, datetime('now')),
                        ('Miscellaneous', 'Other parts and accessories', NULL, 10, datetime('now'))
                ";
                ExecuteNonQuery(connection, seedCategories);
            }
        }

        /// <summary>
        /// Helper method to execute a non-query SQL command
        /// </summary>
        private static void ExecuteNonQuery(SqliteConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Adds a column to a table if it doesn't already exist
        /// </summary>
        private static void AddColumnIfNotExists(SqliteConnection connection, string tableName, string columnName, string columnType)
        {
            if (!ColumnExists(connection, tableName, columnName))
            {
                var addColumn = $@"
                    ALTER TABLE {tableName}
                    ADD COLUMN {columnName} {columnType}";
                ExecuteNonQuery(connection, addColumn);
            }
        }

        /// <summary>
        /// Checks if a column exists in a table
        /// </summary>
        private static bool ColumnExists(SqliteConnection connection, string tableName, string columnName)
        {
            var checkColumn = @"
                SELECT COUNT(*)
                FROM pragma_table_info(@TableName)
                WHERE name = @ColumnName";
            using var command = connection.CreateCommand();
            command.CommandText = checkColumn;
            command.Parameters.AddWithValue("@TableName", tableName);
            command.Parameters.AddWithValue("@ColumnName", columnName);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
    }
}
