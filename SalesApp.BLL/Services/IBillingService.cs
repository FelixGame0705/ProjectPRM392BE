using SalesApp.Models.DTOs;

namespace SalesApp.BLL.Services
{
    public interface IBillingService
    {
        Task<string> StoreBillingDetailsAsync(int orderId, BillingDetailsDto billingDetails);
        Task<BillingDetailsDto?> GetBillingDetailsAsync(int orderId);
        Task<bool> DeleteBillingDetailsAsync(int orderId);
        Task<object> GetOrderWithBillingAsync(int orderId);
    }
} 