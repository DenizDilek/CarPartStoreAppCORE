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
    }
}