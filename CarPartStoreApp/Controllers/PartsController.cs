using CarPartStoreApp.DTOs;
using CarPartStoreApp.Models;
using CarPartStoreApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarPartStoreApp.Controllers
{
    /// <summary>
    /// API Controller for Car Parts CRUD operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PartsController : ControllerBase
    {
        private readonly IDataService _dataService;

        public PartsController(IDataService dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// Get all parts with optional search and category filter
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CarPartDto>>> GetParts(
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null)
        {
            var parts = await _dataService.GetAllPartsAsync();

            // Apply filters if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLowerInvariant();
                parts = parts.Where(p =>
                    (p.PartNumber?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                    (p.Name?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                    (p.Description?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                    (p.Supplier?.ToLowerInvariant().Contains(searchTerm) ?? false)
                ).ToList();
            }

            if (categoryId.HasValue)
            {
                parts = parts.Where(p => p.CategoryId == categoryId.Value).ToList();
            }

            var partDtos = parts.Select(p => MapToDto(p)).ToList();
            return Ok(partDtos);
        }

        /// <summary>
        /// Get a specific part by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CarPartDto?>> GetPart(int id)
        {
            var part = await _dataService.GetPartByIdAsync(id);
            if (part == null)
            {
                return NotFound();
            }
            return Ok(MapToDto(part));
        }

        /// <summary>
        /// Create a new part
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CarPartDto>> CreatePart([FromBody] CreatePartDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var part = MapFromDto(dto);
            var newId = await _dataService.AddPartAsync(part);
            part.Id = newId;

            return CreatedAtAction(nameof(GetPart), part.Id);
        }

        /// <summary>
        /// Update an existing part
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePart(int id, [FromBody] UpdatePartDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingPart = await _dataService.GetPartByIdAsync(id);
            if (existingPart == null)
            {
                return NotFound();
            }

            var part = MapFromDto(dto);
            part.Id = id;

            var success = await _dataService.UpdatePartAsync(part);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a part
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePart(int id)
        {
            var success = await _dataService.DeletePartAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Get parts by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<CarPartDto>>> GetPartsByCategory(int categoryId)
        {
            var parts = await _dataService.GetPartsByCategoryAsync(categoryId);
            var partDtos = parts.Select(p => MapToDto(p)).ToList();
            return Ok(partDtos);
        }

        /// <summary>
        /// Search parts by term
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CarPartDto>>> SearchParts([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Search term cannot be empty");
            }

            var parts = await _dataService.SearchPartsAsync(term);
            var partDtos = parts.Select(p => MapToDto(p)).ToList();
            return Ok(partDtos);
        }

        private CarPartDto MapToDto(CarPart part)
        {
            return new CarPartDto
            {
                Id = part.Id,
                PartNumber = part.PartNumber,
                Name = part.Name,
                Description = part.Description,
                CategoryId = part.CategoryId,
                CategoryName = part.CategoryName,
                CostPrice = part.CostPrice,
                RetailPrice = part.RetailPrice,
                StockQuantity = part.StockQuantity,
                Location = part.Location,
                Supplier = part.Supplier,
                ImagePath = part.ImagePath,
                Model = part.Model,
                ReleaseDate = part.ReleaseDate,
                CreatedDate = part.CreatedDate.ToString("O"),
                LastUpdated = part.LastUpdated?.ToString("O")
            };
        }

        private CarPart MapFromDto(CreatePartDto dto)
        {
            return new CarPart
            {
                PartNumber = dto.PartNumber,
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                CategoryId = dto.CategoryId,
                CostPrice = dto.CostPrice,
                RetailPrice = dto.RetailPrice,
                StockQuantity = dto.StockQuantity,
                Location = dto.Location ?? string.Empty,
                Supplier = dto.Supplier ?? string.Empty,
                ImagePath = dto.ImagePath ?? string.Empty,
                Model = dto.Model,
                ReleaseDate = dto.ReleaseDate,
                CreatedDate = DateTime.Now
            };
        }

        private CarPart MapFromDto(UpdatePartDto dto)
        {
            return new CarPart
            {
                PartNumber = dto.PartNumber,
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                CategoryId = dto.CategoryId,
                CostPrice = dto.CostPrice,
                RetailPrice = dto.RetailPrice,
                StockQuantity = dto.StockQuantity,
                Location = dto.Location ?? string.Empty,
                Supplier = dto.Supplier ?? string.Empty,
                ImagePath = dto.ImagePath ?? string.Empty,
                Model = dto.Model,
                ReleaseDate = dto.ReleaseDate
            };
        }
    }
}
