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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound($"Payment with ID {id} not found");
            }
            return Ok(payment);
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByOrderId(int orderId)
        {
            var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
            return Ok(payments);
        }

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

        [HttpGet("verify/{transactionId}")]
        public async Task<ActionResult<object>> VerifyPayment(string transactionId)
        {
            var isVerified = await _paymentService.VerifyPaymentAsync(transactionId);
            
            return Ok(new
            {
                transactionId = transactionId,
                verified = isVerified,
                message = isVerified ? "Payment verified successfully" : "Payment not found or not completed"
            });
        }

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

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> CreatePayment(CreatePaymentDto createPaymentDto)
        {
            try
            {
                var payment = await _paymentService.CreatePaymentAsync(createPaymentDto);
                return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentID }, payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentDto>> UpdatePayment(int id, PaymentDto paymentDto)
        {
            var updatedPayment = await _paymentService.UpdatePaymentAsync(id, paymentDto);
            if (updatedPayment == null)
            {
                return NotFound($"Payment with ID {id} not found");
            }

            return Ok(updatedPayment);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var success = await _paymentService.DeletePaymentAsync(id);
            if (!success)
            {
                return NotFound($"Payment with ID {id} not found");
            }

            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<PaymentDto>> UpdatePaymentStatus(int id, [FromBody] string status)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound($"Payment with ID {id} not found");
                }

                // Update payment status
                var updateDto = new PaymentDto
                {
                    PaymentID = payment.PaymentID,
                    OrderID = payment.OrderID,
                    Amount = payment.Amount,
                    PaymentDate = payment.PaymentDate,
                    PaymentStatus = status
                };

                var updatedPayment = await _paymentService.UpdatePaymentAsync(id, updateDto);
                
                // Also update order status based on payment status
                if (status == "Completed" && payment.OrderID.HasValue)
                {
                    // You would need to inject IOrderService here or handle this logic differently
                    // For now, let's just return the updated payment
                }

                return Ok(updatedPayment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("simulate-callback/{transactionId}")]
        public async Task<ActionResult<PaymentResponseDto>> SimulateCallback(string transactionId, [FromQuery] string status = "SUCCESS")
        {
            try
            {
                var callback = new PaymentCallbackDto
                {
                    TransactionId = transactionId,
                    Status = status,
                    Message = $"Simulated callback - {status}"
                };

                var response = await _paymentService.HandlePaymentCallbackAsync(callback);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Error simulating callback: {ex.Message}"
                });
            }
        }
    }
} 