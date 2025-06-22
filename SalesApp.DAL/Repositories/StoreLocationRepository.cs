using Microsoft.EntityFrameworkCore;
using SalesApp.DAL.Data;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.DAL.Repositories
{
    public class StoreLocationRepository : GenericRepository<StoreLocation>, IStoreLocationRepository
    {
        public StoreLocationRepository(SalesAppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StoreLocation>> GetNearestStoreLocationsAsync(decimal userLatitude, decimal userLongitude, int limit = 10, double maxDistanceKm = 50)
        {
            // Using Haversine formula for distance calculation
            var allLocations = await GetAllAsync();
            
            var locationsWithDistance = allLocations
                .Select(location => new
                {
                    Location = location,
                    Distance = CalculateDistance((double)userLatitude, (double)userLongitude, 
                                               (double)location.Latitude, (double)location.Longitude)
                })
                .Where(x => x.Distance <= maxDistanceKm)
                .OrderBy(x => x.Distance)
                .Take(limit)
                .Select(x => x.Location);

            return locationsWithDistance;
        }

        public async Task<StoreLocation?> GetNearestStoreLocationAsync(decimal userLatitude, decimal userLongitude)
        {
            var nearestLocations = await GetNearestStoreLocationsAsync(userLatitude, userLongitude, 1, 1000);
            return nearestLocations.FirstOrDefault();
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