using System.ComponentModel.DataAnnotations;

namespace CarPartStoreApp.DTOs
{
    /// <summary>
    /// Data Transfer Object for Car Part API
    /// </summary>
    public class CarPartDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal RetailPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(255)]
        public string? Supplier { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }

        public string? CreatedDate { get; set; }

        public string? LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for creating a new part
    /// </summary>
    public class CreatePartDto
    {
        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal RetailPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(255)]
        public string? Supplier { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }
    }

    /// <summary>
    /// DTO for updating a part
    /// </summary>
    public class UpdatePartDto
    {
        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal RetailPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(255)]
        public string? Supplier { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }
    }
}
