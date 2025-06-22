using Microsoft.AspNetCore.Mvc;
using SalesApp.BLL.Services;
using SalesApp.Models.DTOs;

namespace SalesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreLocationsController : ControllerBase
    {
        private readonly IStoreLocationService _storeLocationService;

        public StoreLocationsController(IStoreLocationService storeLocationService)
        {
            _storeLocationService = storeLocationService;
        }

        /// <summary>
        /// Get all store locations. Optionally provide user location to get distances.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreLocationDto>>> GetAllStoreLocations(
            [FromQuery] decimal? userLatitude = null,
            [FromQuery] decimal? userLongitude = null)
        {
            try
            {
                var locations = await _storeLocationService.GetAllWithDistanceAsync(userLatitude, userLongitude);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific store location by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreLocationDto>> GetStoreLocation(int id)
        {
            try
            {
                var location = await _storeLocationService.GetByIdAsync(id);
                if (location == null)
                {
                    return NotFound($"Store location with ID {id} not found.");
                }
                return Ok(location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get nearest store locations based on user's current location
        /// </summary>
        [HttpPost("nearest")]
        public async Task<ActionResult<IEnumerable<StoreLocationDto>>> GetNearestStoreLocations(NearestStoreRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var locations = await _storeLocationService.GetNearestStoreLocationsAsync(request);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the single nearest store location
        /// </summary>
        [HttpGet("nearest")]
        public async Task<ActionResult<StoreLocationDto>> GetNearestStoreLocation(
            [FromQuery] decimal userLatitude,
            [FromQuery] decimal userLongitude)
        {
            try
            {
                if (userLatitude < -90 || userLatitude > 90)
                {
                    return BadRequest("Latitude must be between -90 and 90");
                }
                if (userLongitude < -180 || userLongitude > 180)
                {
                    return BadRequest("Longitude must be between -180 and 180");
                }

                var location = await _storeLocationService.GetNearestStoreLocationAsync(userLatitude, userLongitude);
                if (location == null)
                {
                    return NotFound("No store locations found.");
                }
                return Ok(location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new store location
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<StoreLocationDto>> CreateStoreLocation(CreateStoreLocationDto createStoreLocationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var location = await _storeLocationService.CreateAsync(createStoreLocationDto);
                return CreatedAtAction(nameof(GetStoreLocation), new { id = location.LocationID }, location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing store location
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<StoreLocationDto>> UpdateStoreLocation(int id, UpdateStoreLocationDto updateStoreLocationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var location = await _storeLocationService.UpdateAsync(id, updateStoreLocationDto);
                if (location == null)
                {
                    return NotFound($"Store location with ID {id} not found.");
                }

                return Ok(location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a store location
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStoreLocation(int id)
        {
            try
            {
                var result = await _storeLocationService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Store location with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get directions URL for Google Maps (for frontend integration)
        /// </summary>
        [HttpGet("{id}/directions")]
        public async Task<ActionResult<object>> GetDirectionsUrl(int id, 
            [FromQuery] decimal? userLatitude = null, 
            [FromQuery] decimal? userLongitude = null)
        {
            try
            {
                var storeLocation = await _storeLocationService.GetByIdAsync(id);
                if (storeLocation == null)
                {
                    return NotFound($"Store location with ID {id} not found.");
                }

                var directionsUrl = "";
                var mapsUrl = "";

                if (userLatitude.HasValue && userLongitude.HasValue)
                {
                    // Google Maps directions URL with origin and destination
                    directionsUrl = $"https://www.google.com/maps/dir/{userLatitude},{userLongitude}/{storeLocation.Latitude},{storeLocation.Longitude}";
                    mapsUrl = $"https://www.google.com/maps/search/?api=1&query={storeLocation.Latitude},{storeLocation.Longitude}";
                }
                else
                {
                    // Just show the store location on map
                    mapsUrl = $"https://www.google.com/maps/search/?api=1&query={storeLocation.Latitude},{storeLocation.Longitude}";
                }

                return Ok(new
                {
                    StoreLocation = storeLocation,
                    DirectionsUrl = directionsUrl,
                    MapsUrl = mapsUrl,
                    GoogleMapsEmbedUrl = $"https://www.google.com/maps/embed/v1/place?key=YOUR_API_KEY&q={storeLocation.Latitude},{storeLocation.Longitude}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 