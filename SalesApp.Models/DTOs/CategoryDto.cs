namespace SalesApp.Models.DTOs
{
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }

    public class CreateCategoryDto
    {
        public string CategoryName { get; set; }
    }
}