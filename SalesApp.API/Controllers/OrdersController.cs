using Microsoft.AspNetCore.Mvc;
using SalesApp.BLL.Services;
using SalesApp.Models.DTOs;

namespace SalesApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IBillingService _billingService;

        public OrdersController(IOrderService orderService, IBillingService billingService)
        {
            _orderService = orderService;
            _billingService = billingService;
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found");
            }

            return Ok(order);
        }

        /// <summary>
        /// Get orders by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByUserId(int userId)
        {
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        /// <summary>
        /// Create a new order with billing details (enhanced version)
        /// </summary>
        [HttpPost("with-billing")]
        public async Task<ActionResult<object>> CreateOrderWithBilling(OrderWithBillingDto orderWithBillingDto)
        {
            try
            {
                // Create the basic order
                var createOrderDto = new CreateOrderDto
                {
                    CartID = orderWithBillingDto.CartID,
                    UserID = orderWithBillingDto.UserID,
                    PaymentMethod = orderWithBillingDto.PaymentMethod,
                    BillingAddress = orderWithBillingDto.BillingDetails.BillingAddress
                };

                var order = await _orderService.CreateOrderAsync(createOrderDto);
                
                // Store billing details separately
                await _billingService.StoreBillingDetailsAsync(order.OrderID, orderWithBillingDto.BillingDetails);
                
                // Return order with billing details
                var result = new
                {
                    Order = order,
                    BillingDetails = orderWithBillingDto.BillingDetails,
                    Message = "Order created successfully with billing details"
                };

                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderID }, result);
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

        /// <summary>
        /// Create a new order (simple version)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(createOrderDto);
                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderID }, order);
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

        /// <summary>
        /// Update an existing order
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderDto>> UpdateOrder(int id, OrderDto orderDto)
        {
            var updatedOrder = await _orderService.UpdateOrderAsync(id, orderDto);
            if (updatedOrder == null)
            {
                return NotFound($"Order with ID {id} not found");
            }

            return Ok(updatedOrder);
        }

        /// <summary>
        /// Delete an order
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            var success = await _orderService.DeleteOrderAsync(id);
            if (!success)
            {
                return NotFound($"Order with ID {id} not found");
            }

            return NoContent();
        }

        /// <summary>
        /// Get order with billing details
        /// </summary>
        [HttpGet("{id}/with-billing")]
        public async Task<ActionResult<object>> GetOrderWithBilling(int id)
        {
            var result = await _billingService.GetOrderWithBillingAsync(id);
            if (result == null)
            {
                return NotFound($"Order with ID {id} not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Get order summary for confirmation screen (enhanced with billing details)
        /// </summary>
        [HttpGet("{id}/summary")]
        public async Task<ActionResult<object>> GetOrderSummary(int id)
        {
            var orderSummary = await _orderService.GetOrderSummaryAsync(id);
            if (orderSummary == null)
            {
                return NotFound($"Order with ID {id} not found");
            }

            // Try to get billing details
            var billingDetails = await _billingService.GetBillingDetailsAsync(id);

            var result = new
            {
                OrderSummary = orderSummary,
                BillingDetails = billingDetails,
                HasBillingDetails = billingDetails != null
            };

            return Ok(result);
        }

        /// <summary>
        /// Add or update billing details for an existing order
        /// </summary>
        [HttpPost("{id}/billing")]
        public async Task<ActionResult<object>> AddBillingDetails(int id, BillingDetailsDto billingDetails)
        {
            // Check if order exists
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found");
            }

            // Store billing details
            var message = await _billingService.StoreBillingDetailsAsync(id, billingDetails);

            return Ok(new
            {
                OrderID = id,
                BillingDetails = billingDetails,
                Message = message
            });
        }

        /// <summary>
        /// Get billing details for an order
        /// </summary>
        [HttpGet("{id}/billing")]
        public async Task<ActionResult<BillingDetailsDto>> GetBillingDetails(int id)
        {
            var billingDetails = await _billingService.GetBillingDetailsAsync(id);
            if (billingDetails == null)
            {
                return NotFound($"Billing details for order {id} not found");
            }

            return Ok(billingDetails);
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, status);
            if (!success)
            {
                return NotFound($"Order with ID {id} not found");
            }

            return Ok(new { message = "Order status updated successfully" });
        }

        /// <summary>
        /// Get available payment methods
        /// </summary>
        [HttpGet("payment-methods")]
        public ActionResult<IEnumerable<object>> GetPaymentMethods()
        {
            var paymentMethods = new[]
            {
                new { Code = "VNPay", Name = "VNPay", Description = "Vietnam Payment Gateway", Icon = "vnpay-icon.png" },
                new { Code = "ZaloPay", Name = "ZaloPay", Description = "Zalo Payment", Icon = "zalopay-icon.png" },
                new { Code = "PayPal", Name = "PayPal", Description = "PayPal International", Icon = "paypal-icon.png" },
                new { Code = "COD", Name = "Cash on Delivery", Description = "Pay when you receive", Icon = "cod-icon.png" }
            };

            return Ok(paymentMethods);
        }
    }
} 