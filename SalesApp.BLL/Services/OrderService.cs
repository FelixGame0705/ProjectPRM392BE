using AutoMapper;
using SalesApp.DAL.UnitOfWork;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Repository<Order>().GetAllAsync();
            var orderDtos = new List<OrderDto>();

            foreach (var order in orders)
            {
                var orderDto = await MapOrderWithRelatedDataAsync(order);
                orderDtos.Add(orderDto);
            }

            return orderDtos;
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id);
            if (order == null) return null;

            return await MapOrderWithRelatedDataAsync(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _unitOfWork.Repository<Order>().FindAsync(o => o.UserID == userId);
            var orderDtos = new List<OrderDto>();

            foreach (var order in orders)
            {
                var orderDto = await MapOrderWithRelatedDataAsync(order);
                orderDtos.Add(orderDto);
            }

            return orderDtos;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            // Get cart to calculate total amount
            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(createOrderDto.CartID);
            if (cart == null)
            {
                throw new ArgumentException("Cart not found");
            }

            // Update cart total to ensure it's accurate
            var cartItems = await _unitOfWork.Repository<CartItem>().FindAsync(ci => ci.CartID == createOrderDto.CartID);
            cart.TotalPrice = cartItems.Sum(ci => ci.Price * ci.Quantity);
            _unitOfWork.Repository<Cart>().Update(cart);

            var order = new Order
            {
                CartID = createOrderDto.CartID,
                UserID = createOrderDto.UserID,
                PaymentMethod = createOrderDto.PaymentMethod,
                BillingAddress = createOrderDto.BillingAddress,
                OrderStatus = "Pending",
                OrderDate = DateTime.Now
            };

            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return await MapOrderWithRelatedDataAsync(order);
        }

        public async Task<OrderDto?> UpdateOrderAsync(int id, OrderDto orderDto)
        {
            var existingOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(id);
            if (existingOrder == null)
            {
                return null;
            }

            existingOrder.PaymentMethod = orderDto.PaymentMethod;
            existingOrder.BillingAddress = orderDto.BillingAddress;
            existingOrder.OrderStatus = orderDto.OrderStatus;

            _unitOfWork.Repository<Order>().Update(existingOrder);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<OrderDto>(existingOrder);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id);
            if (order == null)
            {
                return false;
            }

            _unitOfWork.Repository<Order>().Delete(order);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<OrderSummaryDto?> GetOrderSummaryAsync(int orderId)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null)
            {
                return null;
            }

            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(order.CartID.Value);
            var cartItems = await _unitOfWork.Repository<CartItem>().FindAsync(ci => ci.CartID == cart.CartID);

            var orderItems = new List<OrderItemDto>();
            foreach (var cartItem in cartItems)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(cartItem.ProductID.Value);
                if (product != null)
                {
                    orderItems.Add(new OrderItemDto
                    {
                        ProductName = product.ProductName,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Price,
                        TotalPrice = cartItem.Price * cartItem.Quantity
                    });
                }
            }

            return new OrderSummaryDto
            {
                OrderID = order.OrderID,
                OrderStatus = order.OrderStatus,
                OrderDate = order.OrderDate,
                TotalAmount = cart?.TotalPrice ?? 0,
                PaymentMethod = order.PaymentMethod,
                BillingAddress = order.BillingAddress,
                Items = orderItems
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(orderId);
            if (order == null)
            {
                return false;
            }

            order.OrderStatus = status;
            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private async Task<OrderDto> MapOrderWithRelatedDataAsync(Order order)
        {
            var orderDto = _mapper.Map<OrderDto>(order);
            
            // Load user data
            if (order.UserID.HasValue)
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(order.UserID.Value);
                orderDto.User = _mapper.Map<UserDto>(user);
            }

            // Load cart data and ensure total amount is accurate
            if (order.CartID.HasValue)
            {
                var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(order.CartID.Value);
                if (cart != null)
                {
                    // Ensure cart total is up to date
                    var cartItems = await _unitOfWork.Repository<CartItem>().FindAsync(ci => ci.CartID == cart.CartID);
                    var calculatedTotal = cartItems.Sum(ci => ci.Price * ci.Quantity);
                    
                    // Update cart total if it's different
                    if (cart.TotalPrice != calculatedTotal)
                    {
                        cart.TotalPrice = calculatedTotal;
                        _unitOfWork.Repository<Cart>().Update(cart);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    
                    orderDto.Cart = _mapper.Map<CartDto>(cart);
                    orderDto.TotalAmount = cart.TotalPrice;
                }
            }

            return orderDto;
        }
    }
} 