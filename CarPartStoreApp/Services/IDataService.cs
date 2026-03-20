using CarPartStoreApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarPartStoreApp.Services
{
    /// <summary>
    /// Interface for data access operations
    /// Provides abstraction for database interactions
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Gets all car parts from the database
        /// </summary>
        Task<List<CarPart>> GetAllPartsAsync();

        /// <summary>
        /// Gets a specific car part by ID
        /// </summary>
        Task<CarPart?> GetPartByIdAsync(int id);

        /// <summary>
        /// Adds a new car part to the database
        /// </summary>
        Task<int> AddPartAsync(CarPart part);

        /// <summary>
        /// Updates an existing car part
        /// </summary>
        Task<bool> UpdatePartAsync(CarPart part);

        /// <summary>
        /// Deletes a car part from the database
        /// </summary>
        Task<bool> DeletePartAsync(int id);

        /// <summary>
        /// Gets all categories
        /// </summary>
        Task<List<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Gets a category by ID
        /// </summary>
        Task<Category?> GetCategoryByIdAsync(int id);

        /// <summary>
        /// Gets parts by category
        /// </summary>
        Task<List<CarPart>> GetPartsByCategoryAsync(int categoryId);

        /// <summary>
        /// Searches parts by part number or name
        /// </summary>
        Task<List<CarPart>> SearchPartsAsync(string searchTerm);

        /// <summary>
        /// Gets the database type being used (e.g., "SQLite", "Turso")
        /// </summary>
        string GetDatabaseType();

        /// <summary>
        /// Enables or disables debug tracking for query/response logging
        /// </summary>
        void EnableDebugTracking(bool enabled);
    }

    /// <summary>
    /// Represents the captured details of a database query and its response
    /// </summary>
    public class DatabaseQueryInfo
    {
        /// <summary>
        /// The SQL query that was executed
        /// </summary>
        public string Sql { get; set; } = string.Empty;

        /// <summary>
        /// The parameters used in the query (formatted for display)
        /// </summary>
        public string Parameters { get; set; } = string.Empty;

        /// <summary>
        /// The full request body sent to the database (JSON format)
        /// </summary>
        public string RequestBody { get; set; } = string.Empty;

        /// <summary>
        /// The full response body received from the database (JSON format)
        /// </summary>
        public string ResponseBody { get; set; } = string.Empty;

        /// <summary>
        /// The HTTP status code of the response (for cloud databases)
        /// </summary>
        public int? HttpStatusCode { get; set; }

        /// <summary>
        /// The number of rows affected by the query
        /// </summary>
        public long? AffectedRowCount { get; set; }

        /// <summary>
        /// The last inserted row ID (for INSERT operations)
        /// </summary>
        public long? LastInsertRowId { get; set; }

        /// <summary>
        /// The number of rows returned by the query
        /// </summary>
        public int? RowsReturned { get; set; }

        /// <summary>
        /// The duration of the query in milliseconds
        /// </summary>
        public double? QueryDurationMs { get; set; }

        /// <summary>
        /// Whether the query was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if the query failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Timestamp when the query was executed
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}