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
        public string ProductName { get; set; }
        public string? BriefDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? TechnicalSpecifications { get; set; }
        public decimal Price { get; set; }
        public string? ImageURL { get; set; }
        public int? CategoryID { get; set; }
    }
}