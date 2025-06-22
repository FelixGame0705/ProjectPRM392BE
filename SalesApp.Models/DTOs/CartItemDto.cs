using System.ComponentModel.DataAnnotations;

namespace SalesApp.Models.DTOs
{
    public class CartItemDto
    {
        public int CartItemID { get; set; }
        public int? CartID { get; set; }
        public int? ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageURL { get; set; }
    }

    public class CreateCartItemDto
    {
        public int? CartID { get; set; }
        public int? ProductID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
    }

    public class UpdateCartItemDto
    {
        public int? CartID { get; set; }
        public int? ProductID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int? Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }
    }
}