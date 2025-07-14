using Microsoft.AspNetCore.Mvc;
using SalesApp.BLL.Services;
using SalesApp.Models.DTOs;

namespace SalesApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentGatewayService _paymentGatewayService;

        public PaymentsController(IPaymentService paymentService, IPaymentGatewayService paymentGatewayService)
        {
            _paymentService = paymentService;
            _paymentGatewayService = paymentGatewayService;
        }

        /// <summary>
        /// Process payment - tạo thanh toán và redirect đến gateway
        /// </summary>
        [HttpPost("process")]
        public async Task<ActionResult<PaymentResponseDto>> ProcessPayment(PaymentRequestDto paymentRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _paymentService.ProcessPaymentAsync(paymentRequest);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        /// <summary>
        /// VNPay callback endpoint - xử lý kết quả thanh toán từ VNPay
        /// </summary>
        [HttpGet("callback/vnpay")]
        [HttpPost("callback/vnpay")]
        public async Task<ActionResult<PaymentResponseDto>> HandleVNPayCallback()
        {
            try
            {
                var parameters = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
                var callback = await _paymentGatewayService.VerifyVNPayCallbackAsync(parameters);
                var response = await _paymentService.HandlePaymentCallbackAsync(callback);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Error processing VNPay callback: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get payment status by transaction ID
        /// </summary>
        [HttpGet("status/{transactionId}")]
        public async Task<ActionResult<object>> GetPaymentStatus(string transactionId)
        {
            try
            {
                var paymentId = await _paymentService.GetPaymentIdByTransactionAsync(transactionId);
                
                if (!paymentId.HasValue)
                {
                    return NotFound(new
                    {
                        transactionId = transactionId,
                        found = false,
                        message = "Transaction not found"
                    });
                }

                var payment = await _paymentService.GetPaymentByIdAsync(paymentId.Value);
                
                return Ok(new
                {
                    transactionId = transactionId,
                    found = true,
                    paymentId = paymentId.Value,
                    status = payment?.PaymentStatus,
                    amount = payment?.Amount,
                    paymentDate = payment?.PaymentDate,
                    orderStatus = payment?.Order?.OrderStatus,
                    message = "Payment found"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    transactionId = transactionId,
                    found = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get payments by order ID
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByOrderId(int orderId)
        {
            var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
            return Ok(payments);
        }

        /// <summary>
        /// Check if order has completed payment
        /// </summary>
        [HttpGet("order/{orderId}/status")]
        public async Task<ActionResult<object>> CheckOrderPaymentStatus(int orderId)
        {
            try
            {
                var hasCompletedPayment = await _paymentService.HasCompletedPaymentAsync(orderId);
                
                if (hasCompletedPayment)
                {
                    var completedPayment = await _paymentService.GetCompletedPaymentForOrderAsync(orderId);
                    return Ok(new
                    {
                        orderId = orderId,
                        hasCompletedPayment = true,
                        message = "Order already has a completed payment",
                        payment = completedPayment
                    });
                }

                var allPayments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
                return Ok(new
                {
                    orderId = orderId,
                    hasCompletedPayment = false,
                    message = "Order does not have a completed payment",
                    allPayments = allPayments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    orderId = orderId,
                    hasCompletedPayment = false,
                    message = $"Error checking payment status: {ex.Message}"
                });
            }
        }
    }
} 