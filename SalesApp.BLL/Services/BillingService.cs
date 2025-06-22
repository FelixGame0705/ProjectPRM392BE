using Microsoft.Extensions.Caching.Memory;
using SalesApp.Models.DTOs;

namespace SalesApp.BLL.Services
{
    public class BillingService : IBillingService
    {
        private readonly IMemoryCache _cache;
        private readonly IOrderService _orderService;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24); // 24 hours

        public BillingService(IMemoryCache cache, IOrderService orderService)
        {
            _cache = cache;
            _orderService = orderService;
        }

        public async Task<string> StoreBillingDetailsAsync(int orderId, BillingDetailsDto billingDetails)
        {
            var cacheKey = $"billing_details_{orderId}";
            
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, billingDetails, cacheOptions);
            
            return $"Billing details stored for order {orderId}";
        }

        public async Task<BillingDetailsDto?> GetBillingDetailsAsync(int orderId)
        {
            var cacheKey = $"billing_details_{orderId}";
            
            if (_cache.TryGetValue(cacheKey, out BillingDetailsDto? billingDetails))
            {
                return billingDetails;
            }

            return null;
        }

        public async Task<bool> DeleteBillingDetailsAsync(int orderId)
        {
            var cacheKey = $"billing_details_{orderId}";
            _cache.Remove(cacheKey);
            return true;
        }

        public async Task<object> GetOrderWithBillingAsync(int orderId)
        {
            // Get the order
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return null;
            }

            // Get billing details from cache
            var billingDetails = await GetBillingDetailsAsync(orderId);

            return new
            {
                Order = order,
                BillingDetails = billingDetails,
                HasBillingDetails = billingDetails != null
            };
        }
    }
} 