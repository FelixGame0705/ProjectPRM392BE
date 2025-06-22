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
        [Required]
        public int? UserID { get; set; }

        public int? CartID { get; set; }

        [Required]
        public int? ProductID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        // Removed Price from CreateCartItemDto as it will be taken from Product
    }

    public class UpdateCartItemDto
    {
        public int? CartID { get; set; }
        public int? ProductID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int? Quantity { get; set; }

        // Removed Price from UpdateCartItemDto as it will be taken from Product
    }
}