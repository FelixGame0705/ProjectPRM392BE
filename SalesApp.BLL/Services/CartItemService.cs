using AutoMapper;
using SalesApp.DAL.UnitOfWork;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.BLL.Services
{
    public class CartItemService : ICartItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartItemDto>> GetAllAsync()
        {
            var cartItems = await _unitOfWork.Repository<CartItem>().GetAllAsync();
            return _mapper.Map<IEnumerable<CartItemDto>>(cartItems);
        }

        public async Task<CartItemDto?> GetByIdAsync(int id)
        {
            var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(id);
            return cartItem != null ? _mapper.Map<CartItemDto>(cartItem) : null;
        }

        public async Task<CartItemDto> CreateAsync(CreateCartItemDto createDto)
        {
            var cartItem = _mapper.Map<CartItem>(createDto);
            await _unitOfWork.Repository<CartItem>().AddAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CartItemDto>(cartItem);
        }

        public async Task<CartItemDto?> UpdateAsync(int id, UpdateCartItemDto updateDto)
        {
            var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(id);
            if (cartItem == null) return null;

            _mapper.Map(updateDto, cartItem);
            _unitOfWork.Repository<CartItem>().Update(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CartItemDto>(cartItem);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(id);
            if (cartItem == null) return false;

            _unitOfWork.Repository<CartItem>().Delete(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.Repository<CartItem>().ExistsAsync(id);
        }

        public async Task<IEnumerable<CartItemDto>> GetCartItemsByCartAsync(int cartId)
        {
            var cartItems = await _unitOfWork.Repository<CartItem>().FindAsync(ci => ci.CartID == cartId);
            return _mapper.Map<IEnumerable<CartItemDto>>(cartItems);
        }

        public async Task<bool> RemoveItemFromCartAsync(int cartId, int productId)
        {
            var cartItems = await _unitOfWork.Repository<CartItem>()
                .FindAsync(ci => ci.CartID == cartId && ci.ProductID == productId);
            var cartItem = cartItems.FirstOrDefault();

            if (cartItem == null) return false;

            _unitOfWork.Repository<CartItem>().Delete(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<CartItemDto?> AddOrUpdateCartItemAsync(int cartId, int productId, int quantity)
        {
            var cartItems = await _unitOfWork.Repository<CartItem>()
                .FindAsync(ci => ci.CartID == cartId && ci.ProductID == productId);
            var existingItem = cartItems.FirstOrDefault();

            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                _unitOfWork.Repository<CartItem>().Update(existingItem);
            }
            else
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
                if (product == null) return null;

                existingItem = new CartItem
                {
                    CartID = cartId,
                    ProductID = productId,
                    Quantity = quantity,
                    Price = product.Price
                };
                await _unitOfWork.Repository<CartItem>().AddAsync(existingItem);
            }

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CartItemDto>(existingItem);
        }
    }
}