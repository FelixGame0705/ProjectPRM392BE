namespace SalesApp.BLL.Services
{
    public interface IPaymentMetadataService
    {
        Task<string> StorePaymentMetadataAsync(int paymentId, PaymentMetadataDto metadata);
        Task<PaymentMetadataDto?> GetPaymentMetadataAsync(int paymentId);
        Task<bool> UpdatePaymentMetadataAsync(int paymentId, PaymentMetadataDto metadata);
        Task<bool> DeletePaymentMetadataAsync(int paymentId);
    }

    public class PaymentMetadataDto
    {
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? GatewayTransactionId { get; set; }
        public string? GatewayResponse { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 