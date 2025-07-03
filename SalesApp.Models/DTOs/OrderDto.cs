using System.ComponentModel.DataAnnotations;

namespace SalesApp.Models.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        public int CartID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } // "VNPay", "ZaloPay", "PayPal", "COD"

        [Required]
        [StringLength(255)]
        public string BillingAddress { get; set; }
    }

    public class OrderDto
    {
        public int OrderID { get; set; }
        public int? CartID { get; set; }
        public int? UserID { get; set; }
        public string PaymentMethod { get; set; }
        public string BillingAddress { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Navigation data
        public UserDto? User { get; set; }
        public CartDto? Cart { get; set; }
    }

    public class OrderSummaryDto
    {
        public int OrderID { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string BillingAddress { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
} 