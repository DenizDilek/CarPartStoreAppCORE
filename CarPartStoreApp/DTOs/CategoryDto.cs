using System.ComponentModel.DataAnnotations;

namespace CarPartStoreApp.DTOs
{
    /// <summary>
    /// Data Transfer Object for Category API
    /// </summary>
    public class CategoryDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Name { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public int DisplayOrder { get; set; }

        public string? CreatedDate { get; set; }

        public List<CategoryDto>? SubCategories { get; set; }

        public List<CarPartDto>? Parts { get; set; }
    }
}
