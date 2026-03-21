using System.ComponentModel.DataAnnotations;

namespace CarPartStoreApp.DTOs
{
    /// <summary>
    /// Data Transfer Object for Car Part API
    /// </summary>
    public class CarPartDto
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string? PartNumber { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(2000)]
        public string? ImagePath { get; set; }

        [StringLength(50)]
        public string? Model { get; set; }

        public int? ReleaseDate { get; set; }

        public string? CreatedDate { get; set; }

        public string? LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for creating a new part
    /// </summary>
    public class CreatePartDto
    {
        [StringLength(50)]
        public string? PartNumber { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(2000)]
        public string? ImagePath { get; set; }

        [StringLength(50)]
        public string? Model { get; set; }

        public int? ReleaseDate { get; set; }
    }

    /// <summary>
    /// DTO for updating a part
    /// </summary>
    public class UpdatePartDto
    {
        [StringLength(50)]
        public string? PartNumber { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(2000)]
        public string? ImagePath { get; set; }

        [StringLength(50)]
        public string? Model { get; set; }

        public int? ReleaseDate { get; set; }
    }
}
