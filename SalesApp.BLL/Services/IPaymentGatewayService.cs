using SalesApp.Models.DTOs;

namespace SalesApp.BLL.Services
{
    public interface IPaymentGatewayService
    {
        Task<PaymentResponseDto> CreateVNPayPaymentAsync(PaymentRequestDto request, decimal amount, string orderInfo);
        Task<PaymentResponseDto> CreateZaloPayPaymentAsync(PaymentRequestDto request, decimal amount, string orderInfo);
        Task<PaymentResponseDto> CreatePayPalPaymentAsync(PaymentRequestDto request, decimal amount, string orderInfo);
        Task<PaymentCallbackDto> VerifyVNPayCallbackAsync(Dictionary<string, string> parameters);
        Task<PaymentCallbackDto> VerifyZaloPayCallbackAsync(Dictionary<string, string> parameters);
        Task<PaymentCallbackDto> VerifyPayPalCallbackAsync(Dictionary<string, string> parameters);
        string GenerateTransactionId();
        bool IsPaymentMethodSupported(string paymentMethod);
    }
} 