using System.ComponentModel.DataAnnotations;

namespace SalesApp.Models.DTOs
{
    public class CartDto
    {
        public int CartID { get; set; }
        public int? UserID { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string? UserName { get; set; }
        public List<CartItemDto>? CartItems { get; set; }
    }

    public class CreateCartDto
    {
        public int? UserID { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }
    }

    public class UpdateCartDto
    {
        public int? UserID { get; set; }
        public decimal? TotalPrice { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }
    }
}