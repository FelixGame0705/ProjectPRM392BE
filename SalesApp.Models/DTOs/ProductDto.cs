using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SalesApp.Models.DTOs
{
    public class ProductDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string? BriefDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? TechnicalSpecifications { get; set; }
        public decimal Price { get; set; }
        public string? ImageURL { get; set; }
        public int? CategoryID { get; set; }
        public string? CategoryName { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; }

        [StringLength(255)]
        public string? BriefDescription { get; set; }

        public string? FullDescription { get; set; }

        public string? TechnicalSpecifications { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [StringLength(255)]
        public string? ImageURL { get; set; }

        public int? CategoryID { get; set; }
    }

    public class CreateProductWithImageDto
    {
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; }

        [StringLength(255)]
        public string? BriefDescription { get; set; }

        public string? FullDescription { get; set; }

        public string? TechnicalSpecifications { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public int? CategoryID { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}