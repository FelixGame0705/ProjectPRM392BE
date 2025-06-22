using System.ComponentModel.DataAnnotations;

namespace SalesApp.Models.DTOs
{
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }
    }

    public class UpdateCategoryDto
    {
        [StringLength(100)]
        public string? CategoryName { get; set; }
    }
}