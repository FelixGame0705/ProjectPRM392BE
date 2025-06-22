using SalesApp.Models.DTOs;

namespace SalesApp.BLL.Services
{
    public interface ICartService : IGenericService<CartDto, CreateCartDto, UpdateCartDto>
    {
        Task<IEnumerable<CartDto>> GetCartsByUserAsync(int userId);
        Task<CartDto?> GetCartWithItemsAsync(int cartId);
        Task<bool> UpdateCartTotalAsync(int cartId);
        Task<IEnumerable<CartDto>> GetAllCartsWithItemsAsync();
        Task<CartDto?> GetCartWithItemsByIdAsync(int id);
        Task<CartDto?> UpdateCartStatusAsync(int id, string status);
    }
}