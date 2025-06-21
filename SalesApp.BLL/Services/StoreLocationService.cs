using AutoMapper;
using SalesApp.DAL.UnitOfWork;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.BLL.Services
{
    public class StoreLocationService : IStoreLocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StoreLocationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StoreLocationDto>> GetAllAsync()
        {
            var locations = await _unitOfWork.Repository<StoreLocation>().GetAllAsync();
            return _mapper.Map<IEnumerable<StoreLocationDto>>(locations);
        }

        public async Task<IEnumerable<StoreLocationDto>> GetAllWithDistanceAsync(decimal? userLatitude = null, decimal? userLongitude = null)
        {
            var locations = await _unitOfWork.Repository<StoreLocation>().GetAllAsync();
            var locationDtos = _mapper.Map<IEnumerable<StoreLocationDto>>(locations).ToList();

            if (userLatitude.HasValue && userLongitude.HasValue)
            {
                foreach (var dto in locationDtos)
                {
                    dto.DistanceKm = CalculateDistance((double)userLatitude.Value, (double)userLongitude.Value,
                                                      (double)dto.Latitude, (double)dto.Longitude);
                }
                locationDtos = locationDtos.OrderBy(x => x.DistanceKm).ToList();
            }

            return locationDtos;
        }

        public async Task<StoreLocationDto?> GetByIdAsync(int id)
        {
            var location = await _unitOfWork.Repository<StoreLocation>().GetByIdAsync(id);
            return location != null ? _mapper.Map<StoreLocationDto>(location) : null;
        }

        public async Task<StoreLocationDto> CreateAsync(CreateStoreLocationDto createDto)
        {
            var location = _mapper.Map<StoreLocation>(createDto);

            await _unitOfWork.Repository<StoreLocation>().AddAsync(location);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StoreLocationDto>(location);
        }

        public async Task<StoreLocationDto?> UpdateAsync(int id, UpdateStoreLocationDto updateDto)
        {
            var location = await _unitOfWork.Repository<StoreLocation>().GetByIdAsync(id);
            if (location == null) return null;

            _mapper.Map(updateDto, location);
            _unitOfWork.Repository<StoreLocation>().Update(location);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<StoreLocationDto>(location);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var location = await _unitOfWork.Repository<StoreLocation>().GetByIdAsync(id);
            if (location == null) return false;

            _unitOfWork.Repository<StoreLocation>().Delete(location);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.Repository<StoreLocation>().ExistsAsync(id);
        }

        public async Task<IEnumerable<StoreLocationDto>> GetNearestStoreLocationsAsync(NearestStoreRequestDto request)
        {
            var locations = await _unitOfWork.StoreLocationRepository.GetNearestStoreLocationsAsync(
                request.UserLatitude, request.UserLongitude, request.Limit, request.MaxDistanceKm);

            var locationDtos = _mapper.Map<IEnumerable<StoreLocationDto>>(locations).ToList();

            // Calculate and set distances
            foreach (var dto in locationDtos)
            {
                dto.DistanceKm = CalculateDistance((double)request.UserLatitude, (double)request.UserLongitude,
                                                  (double)dto.Latitude, (double)dto.Longitude);
            }

            return locationDtos.OrderBy(x => x.DistanceKm);
        }

        public async Task<StoreLocationDto?> GetNearestStoreLocationAsync(decimal userLatitude, decimal userLongitude)
        {
            var location = await _unitOfWork.StoreLocationRepository.GetNearestStoreLocationAsync(userLatitude, userLongitude);
            if (location == null) return null;

            var dto = _mapper.Map<StoreLocationDto>(location);
            dto.DistanceKm = CalculateDistance((double)userLatitude, (double)userLongitude,
                                              (double)dto.Latitude, (double)dto.Longitude);
            return dto;
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadius = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
} 