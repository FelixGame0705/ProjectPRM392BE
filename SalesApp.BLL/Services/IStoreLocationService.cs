using SalesApp.Models.DTOs;

namespace SalesApp.BLL.Services
{
    public interface IStoreLocationService : IGenericService<StoreLocationDto, CreateStoreLocationDto, UpdateStoreLocationDto>
    {
        Task<IEnumerable<StoreLocationDto>> GetNearestStoreLocationsAsync(NearestStoreRequestDto request);
        Task<StoreLocationDto?> GetNearestStoreLocationAsync(decimal userLatitude, decimal userLongitude);
        Task<IEnumerable<StoreLocationDto>> GetAllWithDistanceAsync(decimal? userLatitude = null, decimal? userLongitude = null);
    }
} 