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
        public async Task<List<CarPart>> GetAllPartsAsync()
        {
            await Task.CompletedTask; // Keep method signature async

            var parts = new List<CarPart>();

            using var connection = DatabaseConfig.CreateConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.Id, p.PartNumber, p.Name, p.Description, p.CategoryId, c.Name as CategoryName,
                       p.CostPrice, p.RetailPrice, p.StockQuantity, p.Location, p.Supplier, p.ImagePath,
                       p.CreatedDate, p.LastUpdated
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
                       p.CostPrice, p.RetailPrice, p.StockQuantity, p.Location, p.Supplier, p.ImagePath,
                       p.CreatedDate, p.LastUpdated
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
                                   CostPrice, RetailPrice, StockQuantity, Location, Supplier, ImagePath,
                                   CreatedDate, LastUpdated)
                VALUES (@PartNumber, @Name, @Description, @CategoryId,
                        @CostPrice, @RetailPrice, @StockQuantity, @Location, @Supplier, @ImagePath,
                        @CreatedDate, @LastUpdated);
                SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue("@PartNumber", part.PartNumber);
            command.Parameters.AddWithValue("@Name", part.Name);
            command.Parameters.AddWithValue("@Description", part.Description);
            command.Parameters.AddWithValue("@CategoryId", part.CategoryId);
            command.Parameters.AddWithValue("@CostPrice", part.CostPrice);
            command.Parameters.AddWithValue("@RetailPrice", part.RetailPrice);
            command.Parameters.AddWithValue("@StockQuantity", part.StockQuantity);
            command.Parameters.AddWithValue("@Location", part.Location);
            command.Parameters.AddWithValue("@Supplier", part.Supplier);
            command.Parameters.AddWithValue("@ImagePath", (object?)part.ImagePath ?? DBNull.Value);
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
                    RetailPrice = @RetailPrice,
                    StockQuantity = @StockQuantity,
                    Location = @Location,
                    Supplier = @Supplier,
                    ImagePath = @ImagePath,
                    LastUpdated = @LastUpdated
                WHERE Id = @Id
            ";

            command.Parameters.AddWithValue("@PartNumber", part.PartNumber);
            command.Parameters.AddWithValue("@Name", part.Name);
            command.Parameters.AddWithValue("@Description", part.Description);
            command.Parameters.AddWithValue("@CategoryId", part.CategoryId);
            command.Parameters.AddWithValue("@CostPrice", part.CostPrice);
            command.Parameters.AddWithValue("@RetailPrice", part.RetailPrice);
            command.Parameters.AddWithValue("@StockQuantity", part.StockQuantity);
            command.Parameters.AddWithValue("@Location", part.Location);
            command.Parameters.AddWithValue("@Supplier", part.Supplier);
            command.Parameters.AddWithValue("@ImagePath", (object?)part.ImagePath ?? DBNull.Value);
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
                       p.CostPrice, p.RetailPrice, p.StockQuantity, p.Location, p.Supplier, p.ImagePath,
                       p.CreatedDate, p.LastUpdated
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
                       p.CostPrice, p.RetailPrice, p.StockQuantity, p.Location, p.Supplier, p.ImagePath,
                       p.CreatedDate, p.LastUpdated
                FROM Parts p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.PartNumber LIKE @SearchTerm
                   OR p.Name LIKE @SearchTerm
                   OR p.Description LIKE @SearchTerm
                   OR p.Supplier LIKE @SearchTerm
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
        /// </summary>
        private CarPart MapReaderToCarPart(SqliteDataReader reader)
        {
            return new CarPart
            {
                Id = reader.GetInt32(0),
                PartNumber = reader.GetString(1),
                Name = reader.GetString(2),
                Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                CategoryId = reader.GetInt32(4),
                CategoryName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                CostPrice = reader.GetDecimal(6),
                RetailPrice = reader.GetDecimal(7),
                StockQuantity = reader.GetInt32(8),
                Location = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                Supplier = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                ImagePath = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                CreatedDate = DateTime.Parse(reader.GetString(12)),
                LastUpdated = reader.IsDBNull(13) ? (DateTime?)null : DateTime.Parse(reader.GetString(13))
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