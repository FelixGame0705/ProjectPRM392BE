using AutoMapper;
using SalesApp.DAL.UnitOfWork;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.BLL.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IOrderService _orderService;
        private readonly ITransactionMappingService _transactionMappingService;

        public PaymentService(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IPaymentGatewayService paymentGatewayService,
            IOrderService orderService,
            ITransactionMappingService transactionMappingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _paymentGatewayService = paymentGatewayService;
            _orderService = orderService;
            _transactionMappingService = transactionMappingService;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            var payments = await _unitOfWork.Repository<Payment>().GetAllAsync();
            var paymentDtos = new List<PaymentDto>();

            foreach (var payment in payments)
            {
                var paymentDto = await MapPaymentWithRelatedDataAsync(payment);
                paymentDtos.Add(paymentDto);
            }

            return paymentDtos;
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(id);
            if (payment == null) return null;

            return await MapPaymentWithRelatedDataAsync(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(int orderId)
        {
            var payments = await _unitOfWork.Repository<Payment>().FindAsync(p => p.OrderID == orderId);
            var paymentDtos = new List<PaymentDto>();

            foreach (var payment in payments)
            {
                var paymentDto = await MapPaymentWithRelatedDataAsync(payment);
                paymentDtos.Add(paymentDto);
            }

            return paymentDtos;
        }

        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto)
        {
            var payment = new Payment
            {
                OrderID = createPaymentDto.OrderID,
                Amount = createPaymentDto.Amount,
                PaymentDate = DateTime.Now,
                PaymentStatus = "Pending"
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return await MapPaymentWithRelatedDataAsync(payment);
        }

        public async Task<PaymentDto?> UpdatePaymentAsync(int id, PaymentDto paymentDto)
        {
            var existingPayment = await _unitOfWork.Repository<Payment>().GetByIdAsync(id);
            if (existingPayment == null)
            {
                return null;
            }

            existingPayment.Amount = paymentDto.Amount;
            existingPayment.PaymentStatus = paymentDto.PaymentStatus;

            _unitOfWork.Repository<Payment>().Update(existingPayment);
            await _unitOfWork.SaveChangesAsync();

            return await MapPaymentWithRelatedDataAsync(existingPayment);
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(id);
            if (payment == null)
            {
                return false;
            }

            _unitOfWork.Repository<Payment>().Delete(payment);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto paymentRequest)
        {
            try
            {
                // Get order details
                var order = await _unitOfWork.Repository<Order>().GetByIdAsync(paymentRequest.OrderID);
                if (order == null)
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "Order not found"
                    };
                }

                // Check if order already has a completed payment
                var existingPayments = await _unitOfWork.Repository<Payment>()
                    .FindAsync(p => p.OrderID == paymentRequest.OrderID);
                
                var completedPayment = existingPayments.FirstOrDefault(p => 
                    p.PaymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase));
                
                if (completedPayment != null)
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = $"Order #{paymentRequest.OrderID} already has a completed payment. Payment ID: {completedPayment.PaymentID}, Amount: {completedPayment.Amount:C}, Date: {completedPayment.PaymentDate:yyyy-MM-dd HH:mm:ss}"
                    };
                }

                // Check if order status indicates it's already paid
                if (order.OrderStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase) ||
                    order.OrderStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = $"Order #{paymentRequest.OrderID} is already in '{order.OrderStatus}' status and cannot be paid again"
                    };
                }

                // Get cart total amount
                var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(order.CartID.Value);
                if (cart == null)
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "Cart not found"
                    };
                }

                var amount = cart.TotalPrice;
                var orderInfo = $"Payment for Order #{order.OrderID}";

                // Check if payment method is supported
                if (!_paymentGatewayService.IsPaymentMethodSupported(paymentRequest.PaymentMethod))
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "Payment method not supported"
                    };
                }

                // Handle Cash on Delivery
                if (paymentRequest.PaymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase))
                {
                    var payment = await CreatePaymentAsync(new CreatePaymentDto
                    {
                        OrderID = paymentRequest.OrderID,
                        Amount = amount,
                        PaymentMethod = "COD"
                    });

                    await _orderService.UpdateOrderStatusAsync(paymentRequest.OrderID, "Confirmed");

                    return new PaymentResponseDto
                    {
                        Success = true,
                        Message = "Cash on Delivery order confirmed",
                        Payment = payment
                    };
                }

                // Process online payment based on method
                PaymentResponseDto gatewayResponse;
                
                switch (paymentRequest.PaymentMethod.ToUpper())
                {
                    case "VNPAY":
                        gatewayResponse = await _paymentGatewayService.CreateVNPayPaymentAsync(paymentRequest, amount, orderInfo);
                        break;
                    case "ZALOPAY":
                        gatewayResponse = await _paymentGatewayService.CreateZaloPayPaymentAsync(paymentRequest, amount, orderInfo);
                        break;
                    case "PAYPAL":
                        gatewayResponse = await _paymentGatewayService.CreatePayPalPaymentAsync(paymentRequest, amount, orderInfo);
                        break;
                    default:
                        return new PaymentResponseDto
                        {
                            Success = false,
                            Message = "Invalid payment method"
                        };
                }

                if (gatewayResponse.Success)
                {
                    // Create payment record
                    var payment = await CreatePaymentAsync(new CreatePaymentDto
                    {
                        OrderID = paymentRequest.OrderID,
                        Amount = amount,
                        PaymentMethod = paymentRequest.PaymentMethod,
                        PaymentGatewayTransactionId = gatewayResponse.TransactionId
                    });

                    // Store transaction mapping for verification
                    if (!string.IsNullOrEmpty(gatewayResponse.TransactionId))
                    {
                        await _transactionMappingService.StoreTransactionMappingAsync(
                            gatewayResponse.TransactionId, 
                            payment.PaymentID, 
                            paymentRequest.OrderID
                        );
                    }

                    gatewayResponse.Payment = payment;
                }

                return gatewayResponse;
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Error processing payment: {ex.Message}"
                };
            }
        }

        public async Task<PaymentResponseDto> HandlePaymentCallbackAsync(PaymentCallbackDto callback)
        {
            try
            {
                // Find payment using transaction mapping
                var paymentId = await _transactionMappingService.GetPaymentIdByTransactionAsync(callback.TransactionId);
                
                if (!paymentId.HasValue)
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "Payment not found for transaction ID"
                    };
                }

                var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId.Value);
                if (payment == null)
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "Payment record not found"
                    };
                }

                // Update payment status
                payment.PaymentStatus = callback.Status == "SUCCESS" ? "Completed" : "Failed";

                _unitOfWork.Repository<Payment>().Update(payment);

                // Update order status
                if (callback.Status == "SUCCESS")
                {
                    await _orderService.UpdateOrderStatusAsync(payment.OrderID.Value, "Paid");
                }
                else
                {
                    await _orderService.UpdateOrderStatusAsync(payment.OrderID.Value, "Payment Failed");
                }

                await _unitOfWork.SaveChangesAsync();

                return new PaymentResponseDto
                {
                    Success = true,
                    Message = "Payment callback processed successfully",
                    Payment = await MapPaymentWithRelatedDataAsync(payment)
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Error handling payment callback: {ex.Message}"
                };
            }
        }

        public async Task<bool> VerifyPaymentAsync(string transactionId)
        {
            try
            {
                // Find payment using transaction mapping
                var paymentId = await _transactionMappingService.GetPaymentIdByTransactionAsync(transactionId);
                
                if (!paymentId.HasValue)
                {
                    return false; // Transaction not found
                }

                // Get payment and check status
                var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId.Value);
                
                return payment != null && payment.PaymentStatus == "Completed";
            }
            catch
            {
                return false;
            }
        }

        public async Task<int?> GetPaymentIdByTransactionAsync(string transactionId)
        {
            return await _transactionMappingService.GetPaymentIdByTransactionAsync(transactionId);
        }

        public async Task<bool> HasCompletedPaymentAsync(int orderId)
        {
            var existingPayments = await _unitOfWork.Repository<Payment>()
                .FindAsync(p => p.OrderID == orderId);
            
            return existingPayments.Any(p => 
                p.PaymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase));
        }

        public async Task<PaymentDto?> GetCompletedPaymentForOrderAsync(int orderId)
        {
            var existingPayments = await _unitOfWork.Repository<Payment>()
                .FindAsync(p => p.OrderID == orderId);
            
            var completedPayment = existingPayments.FirstOrDefault(p => 
                p.PaymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase));
            
            if (completedPayment == null) return null;
            
            return await MapPaymentWithRelatedDataAsync(completedPayment);
        }

        private async Task<PaymentDto> MapPaymentWithRelatedDataAsync(Payment payment)
        {
            var paymentDto = _mapper.Map<PaymentDto>(payment);
            
            // Load order data with full details
            if (payment.OrderID.HasValue)
            {
                var order = await _unitOfWork.Repository<Order>().GetByIdAsync(payment.OrderID.Value);
                if (order != null)
                {
                    // Use OrderService to get full order data with user and cart
                    var fullOrderDto = await _orderService.GetOrderByIdAsync(order.OrderID);
                    paymentDto.Order = fullOrderDto;
                }
            }

            return paymentDto;
        }
    }
} 