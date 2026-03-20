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
                    PartNumber TEXT UNIQUE,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    CategoryId INTEGER,
                    CostPrice REAL NOT NULL DEFAULT 0,
                    StockQuantity INTEGER NOT NULL DEFAULT 0,
                    Location TEXT,
                    ImagePath TEXT,
<<<<<<< Updated upstream
                    Model TEXT,
                    ReleaseYear INTEGER,
=======
<<<<<<< Updated upstream
=======
                    Model TEXT,
                    Brand TEXT,
                    ReleaseYear INTEGER,
>>>>>>> Stashed changes
>>>>>>> Stashed changes
                    CreatedDate TEXT NOT NULL,
                    LastUpdated TEXT,
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                )";
            ExecuteNonQuery(connection, createPartsTable);

            // Add ImagePath column if it doesn't exist (for existing databases)
            AddColumnIfNotExists(connection, "Parts", "ImagePath", "TEXT");

<<<<<<< Updated upstream
            // Add Model column if it doesn't exist (for existing databases)
            AddColumnIfNotExists(connection, "Parts", "Model", "TEXT");

=======
<<<<<<< Updated upstream
=======
            // Add Model column if it doesn't exist (for existing databases)
            AddColumnIfNotExists(connection, "Parts", "Model", "TEXT");

            // Add Brand column if it doesn't exist (for existing databases)
            AddColumnIfNotExists(connection, "Parts", "Brand", "TEXT");

>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
=======
>>>>>>> Stashed changes
>>>>>>> Stashed changes
            // Create indexes for better performance
            CreateIndexes(connection);

            // Migration: Make PartNumber nullable (remove NOT NULL constraint) for existing databases
            MigratePartNumberToNullable(connection);
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
        /// Migrates existing databases to make PartNumber nullable (removes NOT NULL constraint)
        /// </summary>
        private static void MigratePartNumberToNullable(SqliteConnection connection)
        {
            try
            {
                // Check if the Parts table needs migration by checking SQL for PartNumber column
                var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='Parts'";
                var tableSql = checkCommand.ExecuteScalar()?.ToString();

                if (tableSql != null && tableSql.Contains("PartNumber TEXT NOT NULL"))
                {
                    // Need to migrate - recreate table without NOT NULL constraint
                    // SQLite doesn't support ALTER TABLE to remove constraints, so we rebuild the table
                    var migrateSql = @"
                        BEGIN TRANSACTION;

                        -- Create new table with nullable PartNumber
                        CREATE TABLE Parts_New (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            PartNumber TEXT UNIQUE,
                            Name TEXT NOT NULL,
                            Description TEXT,
                            CategoryId INTEGER,
                            CostPrice REAL NOT NULL DEFAULT 0,
                            StockQuantity INTEGER NOT NULL DEFAULT 0,
                            Location TEXT,
                            ImagePath TEXT,
                            Model TEXT,
                            Brand TEXT,
                            ReleaseYear INTEGER,
                            CreatedDate TEXT NOT NULL,
                            LastUpdated TEXT,
                            FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                        );

                        -- Copy data from old table to new table
                        INSERT INTO Parts_New (Id, PartNumber, Name, Description, CategoryId, CostPrice, StockQuantity, Location, ImagePath, Model, Brand, ReleaseYear, CreatedDate, LastUpdated)
                        SELECT Id, PartNumber, Name, Description, CategoryId, CostPrice, StockQuantity, Location, ImagePath, Model, Brand, ReleaseYear, CreatedDate, LastUpdated
                        FROM Parts;

                        -- Drop old table and rename new table
                        DROP TABLE Parts;
                        ALTER TABLE Parts_New RENAME TO Parts;

                        -- Recreate indexes
                        CREATE INDEX IF NOT EXISTS idx_parts_partnumber ON Parts(PartNumber);
                        CREATE INDEX IF NOT EXISTS idx_parts_category ON Parts(CategoryId);
                        CREATE INDEX IF NOT EXISTS idx_parts_name ON Parts(Name);

                        COMMIT;
                    ";
                    ExecuteNonQuery(connection, migrateSql);
                    // Console.WriteLine("[SQLITE MIGRATION] Made PartNumber nullable (removed NOT NULL constraint)");
                }
            }
            catch (Exception ex)
            {
                // Migration may have already run or table doesn't exist yet - safe to ignore
                // Console.WriteLine($"[SQLITE MIGRATION] PartNumber migration skipped: {ex.Message}");
            }
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
                INSERT OR IGNORE INTO Parts (PartNumber, Name, Description, CategoryId, CostPrice, StockQuantity, Location, Brand, CreatedDate)
                VALUES
<<<<<<< Updated upstream
                    ('ENG-001', 'Oil Filter', 'Standard oil filter for most engines', 8, 5.99, 12.99, 50, 'Aisle 1, Shelf 2', 'AutoParts Inc', datetime('now')),
                    ('ENG-002', 'Air Filter', 'High-flow air filter', 8, 8.99, 18.99, 35, 'Aisle 1, Shelf 3', 'AutoParts Inc', datetime('now')),
<<<<<<< Updated upstream
                    ('TRN-001', 'Transmission Gear', 2, 250.00, 500.00, 8, 'Aisle 2, Shelf 1', 'TransCo', datetime('now')),
                    ('BRK-001', 'Brake Pad Set', 4, 89.99, 120.00, 50, 'Aisle 2, Shelf 1', 'BrakeMaster', datetime('now')),
                    ('FUE-001', 'Fuel Pump Assembly', 5, 300.00, 450.00, 12, 'Aisle 3, Shelf 4', 'SuspensionPro', datetime('now')),
                    ('IGN-001', 'Spark Plug Kit', 7, 25.00, 35.00, 150, 'Aisle 4, Shelf 2', 'ElectroParts', datetime('now')),
                    ('COO-001', 'Radiator Assembly', 8, 125.00, 200.00, 5, 'Aisle 4, Shelf 1', 'RadiatorsRUs', datetime('now')),
                    ('ELE-001', 'Alternator Assembly', 9, 185.00, 350.00, 20, 'Aisle 4, Shelf 2', 'AlternatorsIntl', datetime('now')),
                    ('BOI-001', 'Seat Cushion Set', 10, 75.00, 120.00, 40, 'Aisle 4, Shelf 2', 'SeatMasters', datetime('now'));
=======
                    ('BRK-001', 'Brake Pads (Front)', 'Ceramic brake pads for front wheels', 3, 25.99, 49.99, 20, 'Aisle 2, Shelf 1', 'BrakeMaster', datetime('now')),
                    ('BRK-002', 'Brake Pads (Rear)', 'Semi-metallic brake pads for rear wheels', 3, 22.99, 44.99, 25, 'Aisle 2, Shelf 1', 'BrakeMaster', datetime('now')),
                    ('SUS-001', 'Shock Absorber', 'Universal shock absorber', 4, 45.99, 89.99, 15, 'Aisle 3, Shelf 4', 'SuspensionPro', datetime('now')),
                    ('ELE-001', 'Spark Plug', 'Standard spark plug', 5, 3.99, 8.99, 100, 'Aisle 4, Shelf 2', 'ElectroParts', datetime('now')),
                    ('TRN-001', 'Transmission Fluid', 'Automatic transmission fluid', 2, 12.99, 24.99, 30, 'Aisle 1, Shelf 1', 'TransCo', datetime('now'))
=======
                    ('ENG-001', 'Oil Filter', 'Standard oil filter for most engines', 8, 5.99, 50, 'Aisle 1, Shelf 2', 'Generic', datetime('now')),
                    ('ENG-002', 'Air Filter', 'High-flow air filter', 8, 8.99, 35, 'Aisle 1, Shelf 3', 'Generic', datetime('now')),
                    ('TRN-001', 'Transmission Gear', 2, 250.00, 8, 'Aisle 2, Shelf 1', 'OEM', datetime('now')),
                    ('BRK-001', 'Brake Pad Set', 4, 89.99, 50, 'Aisle 2, Shelf 1', 'Bosch', datetime('now')),
                    ('FUE-001', 'Fuel Pump Assembly', 5, 300.00, 12, 'Aisle 3, Shelf 4', 'ACDelco', datetime('now')),
                    ('IGN-001', 'Spark Plug Kit', 7, 25.00, 150, 'Aisle 4, Shelf 2', 'NGK', datetime('now')),
                    ('COO-001', 'Radiator Assembly', 8, 125.00, 5, 'Aisle 4, Shelf 1', 'Spectra', datetime('now')),
                    ('ELE-001', 'Alternator Assembly', 9, 185.00, 20, 'Aisle 4, Shelf 2', 'Denso', datetime('now')),
                    ('BOI-001', 'Seat Cushion Set', 10, 75.00, 40, 'Aisle 4, Shelf 2', 'Generic', datetime('now'));
>>>>>>> Stashed changes
>>>>>>> Stashed changes
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
