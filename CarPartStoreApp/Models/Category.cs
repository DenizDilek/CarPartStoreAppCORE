using System;
using System.Collections.Generic;

namespace CarPartStoreApp.Models
{
    /// <summary>
    /// Represents a category for organizing car parts
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public List<Category> SubCategories { get; set; } = new List<Category>();

        public List<CarPart> Parts { get; set; } = new List<CarPart>();
    }
}