using System.ComponentModel.DataAnnotations;

namespace SalesApp.Models.DTOs
{
    public class BillingDetailsDto
    {
        [Required]
        [StringLength(255)]
        public string BillingAddress { get; set; }

        [StringLength(255)]
        public string? ShippingAddress { get; set; }

        [StringLength(100)]
        public string? BillingName { get; set; }

        [StringLength(15)]
        public string? BillingPhone { get; set; }

        [StringLength(100)]
        public string? BillingEmail { get; set; }

        public string? Notes { get; set; }
    }

    public class OrderWithBillingDto
    {
        [Required]
        public int CartID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        public BillingDetailsDto BillingDetails { get; set; }
    }
} 