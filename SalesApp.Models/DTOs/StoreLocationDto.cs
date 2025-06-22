using System.ComponentModel.DataAnnotations;

namespace SalesApp.Models.DTOs
{
    public class StoreLocationDto
    {
        public int LocationID { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; }
        
        // Calculated property for distance (will be set by service if user location is provided)
        public double? DistanceKm { get; set; }
    }

    public class CreateStoreLocationDto
    {
        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal Longitude { get; set; }

        [Required]
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string Address { get; set; }
    }

    public class UpdateStoreLocationDto
    {
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal? Longitude { get; set; }

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        public string? Address { get; set; }
    }

    public class NearestStoreRequestDto
    {
        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public decimal UserLatitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public decimal UserLongitude { get; set; }

        [Range(1, 50, ErrorMessage = "Limit must be between 1 and 50")]
        public int Limit { get; set; } = 10;

        [Range(1, 100, ErrorMessage = "Max distance must be between 1 and 100 km")]
        public double MaxDistanceKm { get; set; } = 50;
    }
} 