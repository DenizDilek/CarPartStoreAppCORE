using CarPartStoreApp.DTOs;
using CarPartStoreApp.Models;
using CarPartStoreApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarPartStoreApp.Controllers
{
    /// <summary>
    /// API Controller for Categories CRUD operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IDataService _dataService;

        public CategoriesController(IDataService dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _dataService.GetAllCategoriesAsync();
            var categoryDtos = categories.Select(c => MapToDto(c)).ToList();
            return Ok(categoryDtos);
        }

        /// <summary>
        /// Get a specific category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto?>> GetCategory(int id)
        {
            var category = await _dataService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(MapToDto(category));
        }

        /// <summary>
        /// Get parts within a category
        /// </summary>
        [HttpGet("{id}/parts")]
        public async Task<ActionResult<IEnumerable<CarPartDto>>> GetPartsByCategory(int id)
        {
            var parts = await _dataService.GetPartsByCategoryAsync(id);
            var partDtos = parts.Select(p => new CarPartDto
            {
                Id = p.Id,
                PartNumber = p.PartNumber,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.CategoryName,
                CostPrice = p.CostPrice,
                RetailPrice = p.RetailPrice,
                StockQuantity = p.StockQuantity,
                Location = p.Location,
                Supplier = p.Supplier,
                ImagePath = p.ImagePath,
                CreatedDate = p.CreatedDate.ToString("O"),
                LastUpdated = p.LastUpdated?.ToString("O")
            }).ToList();
            return Ok(partDtos);
        }

        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                DisplayOrder = category.DisplayOrder,
                CreatedDate = category.CreatedDate.ToString("O"),
                SubCategories = category.SubCategories?.Select(sc => MapToDto(sc)).ToList()
            };
        }

        private CategoryDto MapToDto(Category category, List<CarPart> parts)
        {
            var dto = MapToDto(category);
            dto.Parts = parts.Select(p => new CarPartDto
            {
                Id = p.Id,
                PartNumber = p.PartNumber,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.CategoryName,
                CostPrice = p.CostPrice,
                RetailPrice = p.RetailPrice,
                StockQuantity = p.StockQuantity,
                Location = p.Location,
                Supplier = p.Supplier,
                ImagePath = p.ImagePath,
                CreatedDate = p.CreatedDate.ToString("O"),
                LastUpdated = p.LastUpdated?.ToString("O")
            }).ToList();
            return dto;
        }
    }
}
