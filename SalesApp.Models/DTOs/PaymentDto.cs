using System.ComponentModel.DataAnnotations;

namespace SalesApp.Models.DTOs
{
    public class CreatePaymentDto
    {
        [Required]
        public int OrderID { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        public string? PaymentGatewayTransactionId { get; set; }
        public string? PaymentGatewayUrl { get; set; }
    }

    public class PaymentWithMetadataDto
    {
        public PaymentDto Payment { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? GatewayTransactionId { get; set; }
        public string? GatewayResponse { get; set; }
    }

    public class PaymentDto
    {
        public int PaymentID { get; set; }
        public int? OrderID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
        
        public OrderDto? Order { get; set; }
    }

    public class PaymentRequestDto
    {
        [Required]
        public int OrderID { get; set; }

        [Required]
        public string PaymentMethod { get; set; } // "VNPay", "ZaloPay", "PayPal"

        [Required]
        public string ReturnUrl { get; set; }

        public string? CancelUrl { get; set; }
        public string? Language { get; set; } = "vn";
    }

    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? PaymentUrl { get; set; }
        public string? TransactionId { get; set; }
        public PaymentDto? Payment { get; set; }
    }

    public class PaymentCallbackDto
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public string? GatewayTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, string>? AdditionalData { get; set; }
    }

    public class VNPayRequestDto
    {
        public string vnp_Version { get; set; } = "2.1.0";
        public string vnp_Command { get; set; } = "pay";
        public string vnp_TmnCode { get; set; }
        public decimal vnp_Amount { get; set; }
        public string vnp_CurrCode { get; set; } = "VND";
        public string vnp_TxnRef { get; set; }
        public string vnp_OrderInfo { get; set; }
        public string vnp_OrderType { get; set; } = "other";
        public string vnp_Locale { get; set; } = "vn";
        public string vnp_ReturnUrl { get; set; }
        public string vnp_IpAddr { get; set; }
        public string vnp_CreateDate { get; set; }
    }
} 