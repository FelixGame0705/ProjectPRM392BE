using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.BLL.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync();
        Task<PaymentDto?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(int orderId);
        Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto);
        Task<PaymentDto?> UpdatePaymentAsync(int id, PaymentDto paymentDto);
        Task<bool> DeletePaymentAsync(int id);
        Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto paymentRequest);
        Task<PaymentResponseDto> HandlePaymentCallbackAsync(PaymentCallbackDto callback);
        Task<bool> VerifyPaymentAsync(string transactionId);
        Task<int?> GetPaymentIdByTransactionAsync(string transactionId);
    }
} 