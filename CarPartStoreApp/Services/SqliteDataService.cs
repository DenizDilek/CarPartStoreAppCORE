using CarPartStoreApp.Data;
using CarPartStoreApp.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarPartStoreApp.Services
{
    /// <summary>
    /// SQLite implementation of IDataService
    /// Provides full CRUD operations for car parts and categories
    /// </summary>
    public class SqliteDataService : IDataService
    {
        /// <summary>
        /// Gets the database type being used
        /// </summary>
        public string GetDatabaseType() => "SQLite (Local)";

        /// <summary>
        /// Enables or disables debug tracking for query/response logging
        /// </summary>
        public void EnableDebugTracking(bool enabled)
        {
            // SQLite doesn't support query tracking in this implementation
            // This is a no-op for local SQLite
        }
        public async Task<List<CarPart>> GetAllPartsAsync()
        {
            await Task.CompletedTask; // Keep method signature async

            var parts = new List<CarPart>();

            using var connection = DatabaseConfig.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.Id, p.PartNumber, p.Name, p.Description, p.CategoryId, c.Name as CategoryName,
<<<<<<< Updated upstream
                       p.CostPrice, p.RetailPrice, p.StockQuantity, p.Location, p.Supplier, p.ImagePath,
<<<<<<< Updated upstream
                       p.Model, p.ReleaseYear, p.CreatedDate, p.LastUpdated
=======
                       p.CreatedDate, p.LastUpdated
=======
                       p.CostPrice, p.StockQuantity, p.Location, p.ImagePath, p.Model, p.Brand,
                       p.ReleaseYear, p.CreatedDate, p.LastUpdated
>>>>>>> Stashed changes
>>>>>>> Stashed changes
                FROM Parts p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                ORDER BY p.Name
            ";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                parts.Add(MapReaderToCarPart(reader));
            }

            return parts;
        }

        public async Task<CarPart?> GetPartByIdAsync(int id)
        {
            await Task.CompletedTask;

            using var connection = DatabaseConfig.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.Id, p.PartNumber, p.Name, p.Description, p.CategoryId, c.Name as CategoryName,
<<<<<<< Updated upstream
                       p.CostPrice, p.RetailPrice, p.StockQuantity, p.Location, p.Supplier, p.ImagePath,
<<<<<<< Updated upstream
                       p.Model, p.ReleaseYear, p.CreatedDate, p.LastUpdated
=======
                       p.CreatedDate, p.LastUpdated
=======
                       p.CostPrice, p.StockQuantity, p.Location, p.ImagePath, p.Model, p.Brand,
                       p.ReleaseYear, p.CreatedDate, p.LastUpdated
>>>>>>> Stashed changes
>>>>>>> Stashed changes
                FROM Parts p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.Id = @Id
            ";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToCarPart(reader);
            }

            return null;
        }

        public async Task<int> AddPartAsync(CarPart part)
        {
            await Task.CompletedTask;

            using var connection = DatabaseConfig.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Parts (PartNumber, Name, Description, CategoryId,
<<<<<<< Updated upstream
                                   CostPrice, RetailPrice, StockQuantity, Location, Supplier, ImagePath,
                                   Model, ReleaseYear, CreatedDate, LastUpdated)
                VALUES (@PartNumber, @Name, @Description, @CategoryId,
                        @CostPrice, @RetailPrice, @StockQuantity, @Location, @Supplier, @ImagePath,
<<<<<<< Updated upstream
                        @Model, @ReleaseYear, @CreatedDate, @LastUpdated);
=======
                        @CreatedDate, @LastUpdated);
=======
                                   CostPrice, StockQuantity, Location, ImagePath,
                                   Model, Brand, ReleaseYear, CreatedDate, LastUpdated)
                VALUES (@PartNumber, @Name, @Description, @CategoryId,
                        @CostPrice, @StockQuantity, @Location, @ImagePath,
                        @Model, @Brand, @ReleaseYear, @CreatedDate, @LastUpdated);
>>>>>>> Stashed changes
>>>>>>> Stashed changes
                SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue("@PartNumber", part.PartNumber);
            command.Parameters.AddWithValue("@Name", part.Name);
            command.Parameters.AddWithValue("@Description", part.Description);
            command.Parameters.AddWithValue("@CategoryId", part.CategoryId);
            command.Parameters.AddWithValue("@CostPrice", part.CostPrice);
            command.Parameters.AddWithValue("@StockQuantity", part.StockQuantity);
            command.Parameters.AddWithValue("@Location", part.Location);
<<<<<<< Updated upstream
            command.Parameters.AddWithValue("@Supplier", part.Supplier);
            command.Parameters.AddWithValue("@ImagePath", (object?)part.ImagePath ?? DBNull.Value);
<<<<<<< Updated upstream
            command.Parameters.AddWithValue("@Model", (object?)part.Model ?? DBNull.Value);
            command.Parameters.AddWithValue("@ReleaseYear", (object?)part.ReleaseDate ?? DBNull.Value);
=======
=======

            // Combine multiple image paths with space separator
            var imagePathValue = part.ImagePaths.Count > 0
                ? string.Join(" ", part.ImagePaths)
                : string.Empty;
            command.Parameters.AddWithValue("@ImagePath", (object?)imagePathValue ?? DBNull.Value);

            command.Parameters.AddWithValue("@Model", (object?)part.Model ?? DBNull.Value);
            command.Parameters.AddWithValue("@Brand", (object?)part.Brand ?? DBNull.Value);
            command.Parameters.AddWithValue("@ReleaseYear", (object?)part.ReleaseDate ?? DBNull.Value);
>>>>>>> Stashed changes
>>>>>>> Stashed changes
            command.Parameters.AddWithValue("@CreatedDate", part.CreatedDate);
            command.Parameters.AddWithValue("@LastUpdated", (object?)part.LastUpdated ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdatePartAsync(CarPart part)
        {
            await Task.CompletedTask;

            using var connection = DatabaseConfig.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Parts
                SET PartNumber = @PartNumber,
                    Name = @Name,
                    Description = @Description,
                    CategoryId = @CategoryId,
                    CostPrice = @CostPrice,
                    StockQuantity = @StockQuantity,
                    Location = @Location,
                    ImagePath = @ImagePath,
<<<<<<< Updated upstream
                    Model = @Model,
                    ReleaseYear = @ReleaseYear,
=======
<<<<<<< Updated upstream
=======
                    Model = @Model,
                    Brand = @Brand,
                    ReleaseYear = @ReleaseYear,
>>>>>>> Stashed changes
>>>>>>> Stashed changes
                    LastUpdated = @LastUpdated
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@PartNumber", part.PartNumber);
            command.Parameters.AddWithValue("@Name", part.Name);
            command.Parameters.AddWithValue("@Description", part.Description);
            command.Parameters.AddWithValue("@CategoryId", part.CategoryId);
            command.Parameters.AddWithValue("@CostPrice", part.CostPrice);
            command.Parameters.AddWithValue("@StockQuantity", part.StockQuantity);
            command.Parameters.AddWithValue("@Location", part.Location);
<<<<<<< Updated upstream
            command.Parameters.AddWithValue("@Supplier", part.Supplier);
            command.Parameters.AddWithValue("@ImagePath", (object?)part.ImagePath ?? DBNull.Value);
<<<<<<< Updated upstream
            command.Parameters.AddWithValue("@Model", (object?)part.Model ?? DBNull.Value);
            command.Parameters.AddWithValue("@ReleaseYear", (object?)part.ReleaseDate ?? DBNull.Value);
=======
=======

            // Combine multiple image paths with space separator
            var imagePathValue = part.ImagePaths.Count > 0
                ? string.Join(" ", part.ImagePaths)
                : string.Empty;
            command.Parameters.AddWithValue("@ImagePath", (object?)imagePathValue ?? DBNull.Value);

            command.Parameters.AddWithValue("@Model", (object?)part.Model ?? DBNull.Value);
            command.Parameters.AddWithValue("@Brand", (object?)part.Brand ?? DBNull.Value);
            command.Parameters.AddWithValue("@ReleaseYear", (object?)part.ReleaseDate ?? DBNull.Value);
>>>>>>> Stashed changes
>>>>>>> Stashed changes
            command.Parameters.AddWithValue("@LastUpdated", DateTime.Now);
            command.Parameters.AddWithValue("@Id", part.Id);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeletePartAsync(int id)
        {
            await Task.CompletedTask;

            using var connection = DatabaseConfig.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Parts WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            await Task.CompletedTask;

            var categories = new List<Category>();

            using var connection = DatabaseConfig.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Description, ParentCategoryId, DisplayOrder, CreatedDate
                FROM Categories
                ORDER BY DisplayOrder, Name
            ";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categories.Add(MapReaderToCategory(reader));
            }

            return categories;
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            await Task.CompletedTask;

            using var connection = DatabaseConfig.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, Name, Description, ParentCategoryId, DisplayOrder, CreatedDate
                FROM Categories
                WHERE Id = @Id
            ";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToCategory(reader);
            }

            return null;
        }

        public async Task<List<CarPart>> GetPartsByCategoryAsync(int categoryId)
        {
            await Task.CompletedTask;

            var parts = new List<CarPart>();

            using var connection = DatabaseConfig.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.Id, p.PartNumber, p.Name, p.Description, p.CategoryId, c.Name as CategoryName,
<<<<<<< Updated upstream
                       p.CostPrice, p.RetailPrice, p.StockQuantity, p.Location, p.Supplier, p.ImagePath,
<<<<<<< Updated upstream
                       p.Model, p.ReleaseYear, p.CreatedDate, p.LastUpdated
=======
                       p.CreatedDate, p.LastUpdated
=======
                       p.CostPrice, p.StockQuantity, p.Location, p.ImagePath, p.Model, p.Brand,
                       p.ReleaseYear, p.CreatedDate, p.LastUpdated
>>>>>>> Stashed changes
>>>>>>> Stashed changes
                FROM Parts p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.CategoryId = @CategoryId
                ORDER BY p.Name
            ";
            command.Parameters.AddWithValue("@CategoryId", categoryId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                parts.Add(MapReaderToCarPart(reader));
            }

            return parts;
        }

        public async Task<List<CarPart>> SearchPartsAsync(string searchTerm)
        {
            await Task.CompletedTask;

            var parts = new List<CarPart>();

            using var connection = DatabaseConfig.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.Id, p.PartNumber, p.Name, p.Description, p.CategoryId, c.Name as CategoryName,
<<<<<<< Updated upstream
                       p.CostPrice, p.RetailPrice, p.StockQuantity, p.Location, p.Supplier, p.ImagePath,
<<<<<<< Updated upstream
                       p.Model, p.ReleaseYear, p.CreatedDate, p.LastUpdated
=======
                       p.CreatedDate, p.LastUpdated
=======
                       p.CostPrice, p.StockQuantity, p.Location, p.ImagePath, p.Model, p.Brand,
                       p.ReleaseYear, p.CreatedDate, p.LastUpdated
>>>>>>> Stashed changes
>>>>>>> Stashed changes
                FROM Parts p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.PartNumber LIKE @SearchTerm
                   OR p.Name LIKE @SearchTerm
                   OR p.Description LIKE @SearchTerm
                   OR p.Brand LIKE @SearchTerm
                ORDER BY p.Name
            ";
            command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                parts.Add(MapReaderToCarPart(reader));
            }

            return parts;
        }

        /// <summary>
        /// Maps a SQLite data reader to a CarPart object
<<<<<<< Updated upstream
        /// Handles migration from old ReleaseDate TEXT column to new ReleaseYear INTEGER column
        /// </summary>
        private CarPart MapReaderToCarPart(SqliteDataReader reader)
        {
            // Handle ReleaseYear/ReleaseDate column (migration compatibility)
            int? releaseYear = null;
            int releaseYearIndex = 13;
=======
<<<<<<< Updated upstream
        /// </summary>
        private CarPart MapReaderToCarPart(SqliteDataReader reader)
        {
=======
        /// Handles migration from old ReleaseDate TEXT column to new ReleaseYear INTEGER column
        /// Handles parsing multiple image paths from space-separated ImagePath column
        /// </summary>
        private CarPart MapReaderToCarPart(SqliteDataReader reader)
        {
            // Handle ReleaseYear/ReleaseDate column (migration compatibility)
            int? releaseYear = null;
            int releaseYearIndex = 12;
>>>>>>> Stashed changes

            if (reader.IsDBNull(releaseYearIndex))
            {
                releaseYear = null;
            }
            else
            {
                // Check if the column is an INTEGER (new schema) or TEXT (old schema)
                var columnType = reader.GetFieldType(releaseYearIndex);
                if (columnType == typeof(string))
                {
                    // Old schema: Parse year from date string (e.g., "2024-01-15" -> 2024)
                    var dateString = reader.GetString(releaseYearIndex);
                    if (int.TryParse(dateString?.Substring(0, Math.Min(4, dateString?.Length ?? 0)), out var year))
                    {
                        releaseYear = year;
                    }
                }
                else
                {
                    // New schema: Read as integer directly
                    releaseYear = reader.GetInt32(releaseYearIndex);
                }
            }

<<<<<<< Updated upstream
=======
            // Parse ImagePath into multiple paths if needed
            var imagePath = reader.IsDBNull(9) ? string.Empty : reader.GetString(9);
            var imagePaths = new List<string>();
            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                // Split by space or newline
                var paths = imagePath.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                imagePaths.AddRange(paths);
            }

>>>>>>> Stashed changes
>>>>>>> Stashed changes
            return new CarPart
            {
                Id = reader.GetInt32(0),
                PartNumber = reader.GetString(1),
                Name = reader.GetString(2),
                Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                CategoryId = reader.GetInt32(4),
                CategoryName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                CostPrice = reader.GetDecimal(6),
<<<<<<< Updated upstream
                RetailPrice = reader.GetDecimal(7),
                StockQuantity = reader.GetInt32(8),
                Location = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                Supplier = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                ImagePath = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
<<<<<<< Updated upstream
                Model = reader.IsDBNull(12) ? null : reader.GetString(12),
                ReleaseDate = releaseYear,
                CreatedDate = DateTime.Parse(reader.GetString(14)),
                LastUpdated = reader.IsDBNull(15) ? (DateTime?)null : DateTime.Parse(reader.GetString(15))
=======
                CreatedDate = DateTime.Parse(reader.GetString(12)),
                LastUpdated = reader.IsDBNull(13) ? (DateTime?)null : DateTime.Parse(reader.GetString(13))
=======
                StockQuantity = reader.GetInt32(7),
                Location = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                ImagePath = imagePath, // Keep single path for backward compatibility
                ImagePaths = imagePaths, // Set the collection
                Model = reader.IsDBNull(10) ? null : reader.GetString(10),
                Brand = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                ReleaseDate = releaseYear,
                CreatedDate = DateTime.Parse(reader.GetString(13)),
                LastUpdated = reader.IsDBNull(14) ? (DateTime?)null : DateTime.Parse(reader.GetString(14))
>>>>>>> Stashed changes
>>>>>>> Stashed changes
            };
        }

        /// <summary>
        /// Maps a SQLite data reader to a Category object
        /// </summary>
        private Category MapReaderToCategory(SqliteDataReader reader)
        {
            return new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                ParentCategoryId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                DisplayOrder = reader.GetInt32(4),
                CreatedDate = DateTime.Parse(reader.GetString(5))
            };
        }
    }
}