using Microsoft.Data.Sqlite;

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
            // Create tables if they don't exist
            CreateTables();

            // Seed initial data if the database is new
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

        /// <summary>
        /// Creates the database schema if it doesn't exist
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
                    CreatedDate TEXT NOT NULL,
                    LastUpdated TEXT,
                    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
                )";
            ExecuteNonQuery(connection, createPartsTable);

            // Add ImagePath column if it doesn't exist (for existing databases)
            AddColumnIfNotExists(connection, "Parts", "ImagePath", "TEXT");

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
                    ('Electrical', 'Electrical components', NULL, 5, datetime('now')),
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
                    ('BRK-001', 'Brake Pads (Front)', 'Ceramic brake pads for front wheels', 3, 25.99, 49.99, 20, 'Aisle 2, Shelf 1', 'BrakeMaster', datetime('now')),
                    ('BRK-002', 'Brake Pads (Rear)', 'Semi-metallic brake pads for rear wheels', 3, 22.99, 44.99, 25, 'Aisle 2, Shelf 1', 'BrakeMaster', datetime('now')),
                    ('SUS-001', 'Shock Absorber', 'Universal shock absorber', 4, 45.99, 89.99, 15, 'Aisle 3, Shelf 4', 'SuspensionPro', datetime('now')),
                    ('ELE-001', 'Spark Plug', 'Standard spark plug', 5, 3.99, 8.99, 100, 'Aisle 4, Shelf 2', 'ElectroParts', datetime('now')),
                    ('TRN-001', 'Transmission Fluid', 'Automatic transmission fluid', 2, 12.99, 24.99, 30, 'Aisle 1, Shelf 1', 'TransCo', datetime('now'))
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
                        ('Belts & Hoses', 'Drive belts, timing belts, and hoses', NULL, 9, datetime('now')),
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
            // Check if column exists
            var checkColumn = @"
                SELECT COUNT(*)
                FROM pragma_table_info(@TableName)
                WHERE name = @ColumnName";

            using var command = connection.CreateCommand();
            command.CommandText = checkColumn;
            command.Parameters.AddWithValue("@TableName", tableName);
            command.Parameters.AddWithValue("@ColumnName", columnName);

            var columnExists = Convert.ToInt32(command.ExecuteScalar()) > 0;

            if (!columnExists)
            {
                var addColumn = $@"
                    ALTER TABLE {tableName}
                    ADD COLUMN {columnName} {columnType}";
                ExecuteNonQuery(connection, addColumn);
            }
        }
    }
}