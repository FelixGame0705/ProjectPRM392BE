using Microsoft.AspNetCore.Mvc;
using SalesApp.BLL.Services;
using SalesApp.Models.DTOs;

namespace SalesApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartDto>>> GetCarts()
        {
            try
            {
                var carts = await _cartService.GetAllAsync();
                return Ok(carts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // New endpoint: Get all carts with items
        [HttpGet("with-items")]
        public async Task<ActionResult<IEnumerable<CartDto>>> GetAllCartsWithItems()
        {
            try
            {
                var carts = await _cartService.GetAllCartsWithItemsAsync();
                return Ok(carts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CartDto>> GetCart(int id)
        {
            try
            {
                var cart = await _cartService.GetByIdAsync(id);
                if (cart == null)
                    return NotFound($"Cart with ID {id} not found.");

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Updated endpoint: Get cart by ID with items and full product details
        [HttpGet("{id}/with-items")]
        public async Task<ActionResult<CartDto>> GetCartWithItems(int id)
        {
            try
            {
                var cart = await _cartService.GetCartWithItemsByIdAsync(id);
                if (cart == null)
                    return NotFound($"Cart with ID {id} not found.");

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CartDto>>> GetCartsByUser(int userId)
        {
            try
            {
                var carts = await _cartService.GetCartsByUserAsync(userId);
                return Ok(carts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CartDto>> CreateCart(CreateCartDto createCartDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var cart = await _cartService.CreateAsync(createCartDto);
                return CreatedAtAction(nameof(GetCart), new { id = cart.CartID }, cart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CartDto>> UpdateCart(int id, UpdateCartDto updateCartDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedCart = await _cartService.UpdateAsync(id, updateCartDto);
                if (updatedCart == null)
                    return NotFound($"Cart with ID {id} not found.");

                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // New endpoint: Update cart status
        [HttpPut("{id}/status")]
        public async Task<ActionResult<CartDto>> UpdateCartStatus(int id, [FromBody] UpdateCartStatusDto statusDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(statusDto.Status))
                    return BadRequest("Status is required.");

                var updatedCart = await _cartService.UpdateCartStatusAsync(id, statusDto.Status);
                if (updatedCart == null)
                    return NotFound($"Cart with ID {id} not found.");

                return Ok(updatedCart);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Convenience endpoint: Mark cart as done
        [HttpPut("{id}/mark-done")]
        public async Task<ActionResult<CartDto>> MarkCartAsDone(int id)
        {
            try
            {
                var updatedCart = await _cartService.UpdateCartStatusAsync(id, "done");
                if (updatedCart == null)
                    return NotFound($"Cart with ID {id} not found.");

                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}/update-total")]
        public async Task<IActionResult> UpdateCartTotal(int id)
        {
            try
            {
                var updated = await _cartService.UpdateCartTotalAsync(id);
                if (!updated)
                    return NotFound($"Cart with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            try
            {
                var deleted = await _cartService.DeleteAsync(id);
                if (!deleted)
                    return NotFound($"Cart with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpHead("{id}")]
        public async Task<IActionResult> CartExists(int id)
        {
            try
            {
                var exists = await _cartService.ExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    // DTO for updating cart status
    public class UpdateCartStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}