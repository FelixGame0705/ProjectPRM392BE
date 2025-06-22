using Microsoft.AspNetCore.Mvc;
using SalesApp.BLL.Services;
using SalesApp.Models.DTOs;

namespace SalesApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartItemsController : ControllerBase
    {
        private readonly ICartItemService _cartItemService;

        public CartItemsController(ICartItemService cartItemService)
        {
            _cartItemService = cartItemService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCartItems()
        {
            try
            {
                var cartItems = await _cartItemService.GetAllAsync();
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CartItemDto>> GetCartItem(int id)
        {
            try
            {
                var cartItem = await _cartItemService.GetByIdAsync(id);
                if (cartItem == null)
                    return NotFound($"Cart item with ID {id} not found.");

                return Ok(cartItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("cart/{cartId}")]
        public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCartItemsByCart(int cartId)
        {
            try
            {
                var cartItems = await _cartItemService.GetCartItemsByCartAsync(cartId);
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CartItemDto>> CreateCartItem(CreateCartItemDto createCartItemDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var cartItem = await _cartItemService.CreateAsync(createCartItemDto);
                return CreatedAtAction(nameof(GetCartItem), new { id = cartItem.CartItemID }, cartItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("add-or-update")]
        public async Task<ActionResult<CartItemDto>> AddOrUpdateCartItem(
            [FromQuery] int cartId,
            [FromQuery] int productId,
            [FromQuery] int quantity)
        {
            try
            {
                if (cartId <= 0 || productId <= 0 || quantity <= 0)
                    return BadRequest("Invalid parameters.");

                var cartItem = await _cartItemService.AddOrUpdateCartItemAsync(cartId, productId, quantity);
                if (cartItem == null)
                    return NotFound("Product not found.");

                return Ok(cartItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CartItemDto>> UpdateCartItem(int id, UpdateCartItemDto updateCartItemDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedCartItem = await _cartItemService.UpdateAsync(id, updateCartItemDto);
                if (updatedCartItem == null)
                    return NotFound($"Cart item with ID {id} not found.");

                return Ok(updatedCartItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            try
            {
                var deleted = await _cartItemService.DeleteAsync(id);
                if (!deleted)
                    return NotFound($"Cart item with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("cart/{cartId}/product/{productId}")]
        public async Task<IActionResult> RemoveItemFromCart(int cartId, int productId)
        {
            try
            {
                var removed = await _cartItemService.RemoveItemFromCartAsync(cartId, productId);
                if (!removed)
                    return NotFound("Cart item not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpHead("{id}")]
        public async Task<IActionResult> CartItemExists(int id)
        {
            try
            {
                var exists = await _cartItemService.ExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
