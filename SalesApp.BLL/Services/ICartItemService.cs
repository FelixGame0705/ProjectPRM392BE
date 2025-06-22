using SalesApp.Models.DTOs;

namespace SalesApp.BLL.Services
{
    public interface ICartItemService : IGenericService<CartItemDto, CreateCartItemDto, UpdateCartItemDto>
    {
        Task<IEnumerable<CartItemDto>> GetCartItemsByCartAsync(int cartId);
        Task<bool> RemoveItemFromCartAsync(int cartId, int productId);
        Task<CartItemDto?> AddOrUpdateCartItemAsync(int cartId, int productId, int quantity);
    }
}
