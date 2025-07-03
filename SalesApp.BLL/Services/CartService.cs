using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SalesApp.DAL.UnitOfWork;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.BLL.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CartDto>> GetAllAsync()
        {
            var carts = await _unitOfWork.Repository<Cart>().GetAllAsync();
            return _mapper.Map<IEnumerable<CartDto>>(carts);
        }

        public async Task<CartDto?> GetByIdAsync(int id)
        {
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(id);
            return cart != null ? _mapper.Map<CartDto>(cart) : null;
        }

        public async Task<CartDto> CreateAsync(CreateCartDto createDto)
        {
            var cart = _mapper.Map<Cart>(createDto);
            await _unitOfWork.Repository<Cart>().AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CartDto>(cart);
        }

        public async Task<CartDto?> UpdateAsync(int id, UpdateCartDto updateDto)
        {
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(id);
            if (cart == null)
                return null;

            _mapper.Map(updateDto, cart);
            _unitOfWork.Repository<Cart>().Update(cart);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<CartDto>(cart);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(id);
            if (cart == null)
                return false;

            _unitOfWork.Repository<Cart>().Delete(cart);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.Repository<Cart>().ExistsAsync(id);
        }

        public async Task<IEnumerable<CartDto>> GetCartsByUserAsync(int userId)
        {
            var carts = await _unitOfWork.Repository<Cart>().FindAsync(c => c.UserID == userId);
            var cartDtos = new List<CartDto>();

            foreach (var cart in carts)
            {
                var cartDto = _mapper.Map<CartDto>(cart);
                var cartItems = await _unitOfWork
                    .Repository<CartItem>()
                    .FindAsync(ci => ci.CartID == cart.CartID);

                // Map cart items with product details
                var cartItemDtos = new List<CartItemDto>();
                foreach (var item in cartItems)
                {
                    var product = await _unitOfWork
                        .Repository<Product>()
                        .GetByIdAsync(item.ProductID.Value);
                    var itemDto = _mapper.Map<CartItemDto>(item);
                    if (product != null)
                    {
                        itemDto.ProductName = product.ProductName;
                        itemDto.ProductImageURL = product.ImageURL;
                    }
                    cartItemDtos.Add(itemDto);
                }

                cartDto.CartItems = cartItemDtos;
                cartDtos.Add(cartDto);
            }

            return cartDtos;
        }

        public async Task<CartDto?> GetCartWithItemsAsync(int cartId)
        {
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(cartId);
            if (cart == null)
                return null;

            var cartItems = await _unitOfWork
                .Repository<CartItem>()
                .FindAsync(ci => ci.CartID == cartId);
            var cartDto = _mapper.Map<CartDto>(cart);
            cartDto.CartItems = _mapper.Map<List<CartItemDto>>(cartItems);
            return cartDto;
        }

        public async Task<bool> UpdateCartTotalAsync(int cartId)
        {
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(cartId);
            if (cart == null)
                return false;

            var cartItems = await _unitOfWork
                .Repository<CartItem>()
                .FindAsync(ci => ci.CartID == cartId);
            cart.TotalPrice = cartItems.Sum(ci => ci.Price * ci.Quantity);

            _unitOfWork.Repository<Cart>().Update(cart);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CartDto>> GetAllCartsWithItemsAsync()
        {
            var carts = await _unitOfWork.Repository<Cart>().GetAllAsync();
            var cartDtos = new List<CartDto>();

            foreach (var cart in carts)
            {
                var cartDto = _mapper.Map<CartDto>(cart);
                var cartItems = await _unitOfWork
                    .Repository<CartItem>()
                    .FindAsync(ci => ci.CartID == cart.CartID);

                // Map cart items with product details
                var cartItemDtos = new List<CartItemDto>();
                foreach (var item in cartItems)
                {
                    var product = await _unitOfWork
                        .Repository<Product>()
                        .GetByIdAsync(item.ProductID.Value);
                    var itemDto = _mapper.Map<CartItemDto>(item);
                    if (product != null)
                    {
                        itemDto.ProductName = product.ProductName;
                        itemDto.ProductImageURL = product.ImageURL;
                    }
                    cartItemDtos.Add(itemDto);
                }

                cartDto.CartItems = cartItemDtos;
                cartDtos.Add(cartDto);
            }

            return cartDtos;
        }

        public async Task<CartDto?> GetCartWithItemsByIdAsync(int id)
        {
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(id);
            if (cart == null)
                return null;

            var cartDto = _mapper.Map<CartDto>(cart);
            var cartItems = await _unitOfWork
                .Repository<CartItem>()
                .FindAsync(ci => ci.CartID == id);

            var cartItemDtos = new List<CartItemDto>();
            foreach (var item in cartItems)
            {
                var product = await _unitOfWork
                    .Repository<Product>()
                    .GetByIdAsync(item.ProductID.Value);
                var itemDto = _mapper.Map<CartItemDto>(item);
                if (product != null)
                {
                    itemDto.ProductName = product.ProductName;
                    itemDto.ProductImageURL = product.ImageURL;
                }
                cartItemDtos.Add(itemDto);
            }

            cartDto.CartItems = cartItemDtos;

            if (cart.UserID.HasValue)
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(cart.UserID.Value);
                if (user != null)
                {
                    cartDto.UserName = user.Username;
                }
            }

            return cartDto;
        }

        public async Task<CartDto?> UpdateCartStatusAsync(int id, string status)
        {
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(id);
            if (cart == null)
                return null;

            // Validate status
            var validStatuses = new[] { "active", "done", "cancelled", "pending" };
            if (!validStatuses.Contains(status.ToLower()))
            {
                throw new ArgumentException(
                    $"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}"
                );
            }

            cart.Status = status;
            _unitOfWork.Repository<Cart>().Update(cart);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CartDto>(cart);
        }
    }
}
