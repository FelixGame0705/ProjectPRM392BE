using Microsoft.Extensions.Caching.Memory;

namespace SalesApp.BLL.Services
{
    public interface ITransactionMappingService
    {
        Task StoreTransactionMappingAsync(string transactionId, int paymentId, int orderId);
        Task<int?> GetPaymentIdByTransactionAsync(string transactionId);
        Task<int?> GetOrderIdByTransactionAsync(string transactionId);
        Task RemoveTransactionMappingAsync(string transactionId);
    }

    public class TransactionMappingService : ITransactionMappingService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);

        public TransactionMappingService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task StoreTransactionMappingAsync(string transactionId, int paymentId, int orderId)
        {
            var mapping = new TransactionMapping
            {
                PaymentId = paymentId,
                OrderId = orderId,
                CreatedAt = DateTime.Now
            };

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set($"txn_{transactionId}", mapping, cacheOptions);
        }

        public async Task<int?> GetPaymentIdByTransactionAsync(string transactionId)
        {
            if (_cache.TryGetValue($"txn_{transactionId}", out TransactionMapping mapping))
            {
                return mapping.PaymentId;
            }
            return null;
        }

        public async Task<int?> GetOrderIdByTransactionAsync(string transactionId)
        {
            if (_cache.TryGetValue($"txn_{transactionId}", out TransactionMapping mapping))
            {
                return mapping.OrderId;
            }
            return null;
        }

        public async Task RemoveTransactionMappingAsync(string transactionId)
        {
            _cache.Remove($"txn_{transactionId}");
        }

        private class TransactionMapping
        {
            public int PaymentId { get; set; }
            public int OrderId { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
} 