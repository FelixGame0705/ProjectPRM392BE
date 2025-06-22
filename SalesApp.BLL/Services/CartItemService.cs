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
            // Lấy thông tin product để có giá
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(createDto.ProductID.Value);
            if (product == null)
                throw new ArgumentException("Product not found");

            // Tìm hoặc tạo cart cho user
            var cart = await GetOrCreateCartForUser(createDto.UserID.Value);

            // Kiểm tra xem đã có cart item này chưa
            var existingCartItems = await _unitOfWork.Repository<CartItem>()
                .FindAsync(ci => ci.CartID == cart.CartID && ci.ProductID == createDto.ProductID);
            var existingCartItem = existingCartItems.FirstOrDefault();

            CartItem cartItem;
            if (existingCartItem != null)
            {
                // Cập nhật quantity nếu đã tồn tại
                existingCartItem.Quantity += createDto.Quantity;
                existingCartItem.Price = product.Price; // Cập nhật giá mới nhất
                _unitOfWork.Repository<CartItem>().Update(existingCartItem);
                cartItem = existingCartItem;
            }
            else
            {
                // Tạo cart item mới
                cartItem = new CartItem
                {
                    CartID = cart.CartID,
                    ProductID = createDto.ProductID,
                    Quantity = createDto.Quantity,
                    Price = product.Price // Luôn lấy giá từ product
                };
                await _unitOfWork.Repository<CartItem>().AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();

            // Cập nhật tổng tiền của cart
            await UpdateCartTotal(cart.CartID);

            return _mapper.Map<CartItemDto>(cartItem);
        }

        public async Task<CartItemDto?> UpdateAsync(int id, UpdateCartItemDto updateDto)
        {
            var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(id);
            if (cartItem == null) return null;

            // Nếu có cập nhật ProductID, lấy giá mới
            if (updateDto.ProductID.HasValue && updateDto.ProductID != cartItem.ProductID)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(updateDto.ProductID.Value);
                if (product == null)
                    throw new ArgumentException("Product not found");
                cartItem.Price = product.Price;
                cartItem.ProductID = updateDto.ProductID;
            }

            // Cập nhật quantity nếu có
            if (updateDto.Quantity.HasValue)
            {
                cartItem.Quantity = updateDto.Quantity.Value;
            }

            _unitOfWork.Repository<CartItem>().Update(cartItem);
            await _unitOfWork.SaveChangesAsync();

            // Cập nhật tổng tiền của cart
            await UpdateCartTotal(cartItem.CartID.Value);

            return _mapper.Map<CartItemDto>(cartItem);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(id);
            if (cartItem == null) return false;

            var cartId = cartItem.CartID;
            _unitOfWork.Repository<CartItem>().Delete(cartItem);
            await _unitOfWork.SaveChangesAsync();

            // Cập nhật tổng tiền của cart sau khi xóa
            if (cartId.HasValue)
                await UpdateCartTotal(cartId.Value);

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

            // Cập nhật tổng tiền của cart
            await UpdateCartTotal(cartId);

            return true;
        }

        public async Task<CartItemDto?> AddOrUpdateCartItemAsync(int cartId, int productId, int quantity)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null) return null;

            var cartItems = await _unitOfWork.Repository<CartItem>()
                .FindAsync(ci => ci.CartID == cartId && ci.ProductID == productId);
            var existingItem = cartItems.FirstOrDefault();

            if (existingItem != null)
            {
                existingItem.Quantity = quantity;
                existingItem.Price = product.Price; // Cập nhật giá mới nhất
                _unitOfWork.Repository<CartItem>().Update(existingItem);
            }
            else
            {
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

            // Cập nhật tổng tiền của cart
            await UpdateCartTotal(cartId);

            return _mapper.Map<CartItemDto>(existingItem);
        }

        // Phương thức để tìm hoặc tạo cart cho user
        private async Task<Cart> GetOrCreateCartForUser(int userId)
        {
            // Tìm cart hiện tại của user có status khác "done"
            var existingCarts = await _unitOfWork.Repository<Cart>()
                .FindAsync(c => c.UserID == userId && c.Status != "done");
            var activeCart = existingCarts.FirstOrDefault();

            if (activeCart != null)
            {
                return activeCart;
            }

            // Tạo cart mới nếu không có cart active
            var newCart = new Cart
            {
                UserID = userId,
                TotalPrice = 0,
                Status = "active"
            };

            await _unitOfWork.Repository<Cart>().AddAsync(newCart);
            await _unitOfWork.SaveChangesAsync();

            return newCart;
        }

        // Phương thức để cập nhật tổng tiền của cart
        private async Task UpdateCartTotal(int cartId)
        {
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(cartId);
            if (cart == null) return;

            var cartItems = await _unitOfWork.Repository<CartItem>().FindAsync(ci => ci.CartID == cartId);
            cart.TotalPrice = cartItems.Sum(ci => ci.Price * ci.Quantity);

            _unitOfWork.Repository<Cart>().Update(cart);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}