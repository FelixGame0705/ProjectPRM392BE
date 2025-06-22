using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.DAL.Repositories
{
    public interface IStoreLocationRepository : IGenericRepository<StoreLocation>
    {
        Task<IEnumerable<StoreLocation>> GetNearestStoreLocationsAsync(decimal userLatitude, decimal userLongitude, int limit = 10, double maxDistanceKm = 50);
        Task<StoreLocation?> GetNearestStoreLocationAsync(decimal userLatitude, decimal userLongitude);
    }
} 