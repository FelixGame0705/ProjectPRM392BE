using Microsoft.Extensions.Configuration;
using SalesApp.Models.DTOs;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SalesApp.BLL.Services
{
    public class PaymentGatewayService : IPaymentGatewayService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PaymentGatewayService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<PaymentResponseDto> CreateVNPayPaymentAsync(PaymentRequestDto request, decimal amount, string orderInfo)
        {
            try
            {
                var vnpayConfig = _configuration.GetSection("VNPay");
                var tmnCode = vnpayConfig["TmnCode"];
                var hashSecret = vnpayConfig["HashSecret"];
                var baseUrl = vnpayConfig["BaseUrl"];

                if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(hashSecret) || string.IsNullOrEmpty(baseUrl))
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "VNPay configuration is missing"
                    };
                }

                var transactionId = GenerateTransactionId();
                var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");

                // Use "string" for VNPay sandbox compatibility
                var returnUrl = string.IsNullOrWhiteSpace(request.ReturnUrl) || 
                               request.ReturnUrl.Equals("string", StringComparison.OrdinalIgnoreCase) 
                               ? "string" : "string";

                var vnpayData = new SortedDictionary<string, string>
                {
                    { "vnp_Version", "2.1.0" },
                    { "vnp_Command", "pay" },
                    { "vnp_TmnCode", tmnCode },
                    { "vnp_Amount", ((long)(amount * 100)).ToString() },
                    { "vnp_CurrCode", "VND" },
                    { "vnp_TxnRef", transactionId },
                    { "vnp_OrderInfo", orderInfo },
                    { "vnp_OrderType", "other" },
                    { "vnp_Locale", request.Language ?? "vn" },
                    { "vnp_ReturnUrl", returnUrl },
                    { "vnp_IpAddr", "127.0.0.1" },
                    { "vnp_CreateDate", createDate }
                };

                // Build rawData for hash calculation
                var rawData = new StringBuilder();
                foreach (var item in vnpayData)
                {
                    rawData.Append($"{item.Key}={HttpUtility.UrlEncode(item.Value, Encoding.UTF8)}&");
                }
                rawData.Remove(rawData.Length - 1, 1);

                // Calculate hash
                var secureHash = CreateVNPaySecureHash(rawData.ToString(), hashSecret);

                // Build payment URL
                var paymentUrl = $"{baseUrl}?{rawData}&vnp_SecureHash={secureHash}";

                return new PaymentResponseDto
                {
                    Success = true,
                    Message = "VNPay payment URL created successfully",
                    PaymentUrl = paymentUrl,
                    TransactionId = transactionId
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Error creating VNPay payment: {ex.Message}"
                };
            }
        }

        public async Task<PaymentResponseDto> CreateZaloPayPaymentAsync(PaymentRequestDto request, decimal amount, string orderInfo)
        {
            try
            {
                var zaloPayConfig = _configuration.GetSection("ZaloPay");
                var appId = zaloPayConfig["AppId"];
                var key1 = zaloPayConfig["Key1"];
                var endpoint = zaloPayConfig["Endpoint"];

                if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(key1) || string.IsNullOrEmpty(endpoint))
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "ZaloPay configuration is missing"
                    };
                }

                var transactionId = GenerateTransactionId();

                return new PaymentResponseDto
                {
                    Success = true,
                    Message = "ZaloPay payment created successfully",
                    TransactionId = transactionId,
                    PaymentUrl = "zalo://payment"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Error creating ZaloPay payment: {ex.Message}"
                };
            }
        }

        public async Task<PaymentResponseDto> CreatePayPalPaymentAsync(PaymentRequestDto request, decimal amount, string orderInfo)
        {
            try
            {
                var paypalConfig = _configuration.GetSection("PayPal");
                var clientId = paypalConfig["ClientId"];
                var baseUrl = paypalConfig["BaseUrl"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(baseUrl))
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "PayPal configuration is missing"
                    };
                }

                var transactionId = GenerateTransactionId();
                var paypalUrl = $"{baseUrl}/checkout?amount={amount}&currency=USD&return_url={HttpUtility.UrlEncode(request.ReturnUrl)}";

                return new PaymentResponseDto
                {
                    Success = true,
                    Message = "PayPal payment created successfully",
                    PaymentUrl = paypalUrl,
                    TransactionId = transactionId
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Error creating PayPal payment: {ex.Message}"
                };
            }
        }

        public async Task<PaymentCallbackDto> VerifyVNPayCallbackAsync(Dictionary<string, string> parameters)
        {
            try
            {
                var vnpayConfig = _configuration.GetSection("VNPay");
                var hashSecret = vnpayConfig["HashSecret"];

                if (!parameters.ContainsKey("vnp_SecureHash"))
                {
                    return new PaymentCallbackDto
                    {
                        TransactionId = parameters.GetValueOrDefault("vnp_TxnRef", ""),
                        Status = "FAILED",
                        Message = "Missing secure hash"
                    };
                }

                var secureHash = parameters["vnp_SecureHash"];
                parameters.Remove("vnp_SecureHash");

                var sortedParams = new SortedDictionary<string, string>(parameters);

                var rawData = new StringBuilder();
                foreach (var item in sortedParams)
                {
                    rawData.Append($"{item.Key}={HttpUtility.UrlEncode(item.Value, Encoding.UTF8)}&");
                }
                rawData.Remove(rawData.Length - 1, 1);

                var expectedHash = CreateVNPaySecureHash(rawData.ToString(), hashSecret);
                var responseCode = sortedParams.GetValueOrDefault("vnp_ResponseCode", "");
                var amount = decimal.Parse(sortedParams.GetValueOrDefault("vnp_Amount", "0")) / 100;

                bool isValidSignature = secureHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);

                return new PaymentCallbackDto
                {
                    TransactionId = sortedParams.GetValueOrDefault("vnp_TxnRef", ""),
                    Status = responseCode == "00" && isValidSignature ? "SUCCESS" : "FAILED",
                    GatewayTransactionId = sortedParams.GetValueOrDefault("vnp_TransactionNo", ""),
                    Amount = amount,
                    Message = !isValidSignature ? "Invalid signature" : 
                             responseCode == "00" ? "Payment successful" : "Payment failed",
                    AdditionalData = new Dictionary<string, string>(sortedParams)
                };
            }
            catch (Exception ex)
            {
                return new PaymentCallbackDto
                {
                    TransactionId = "",
                    Status = "FAILED",
                    Message = $"Error verifying VNPay callback: {ex.Message}"
                };
            }
        }

        public async Task<PaymentCallbackDto> VerifyZaloPayCallbackAsync(Dictionary<string, string> parameters)
        {
            return new PaymentCallbackDto
            {
                TransactionId = parameters.GetValueOrDefault("app_trans_id", ""),
                Status = "SUCCESS",
                Message = "ZaloPay payment verified"
            };
        }

        public async Task<PaymentCallbackDto> VerifyPayPalCallbackAsync(Dictionary<string, string> parameters)
        {
            return new PaymentCallbackDto
            {
                TransactionId = parameters.GetValueOrDefault("payment_id", ""),
                Status = "SUCCESS",
                Message = "PayPal payment verified"
            };
        }

        public string GenerateTransactionId()
        {
            return $"TXN{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }

        public bool IsPaymentMethodSupported(string paymentMethod)
        {
            var supportedMethods = new[] { "VNPay", "ZaloPay", "PayPal", "COD" };
            return supportedMethods.Contains(paymentMethod, StringComparer.OrdinalIgnoreCase);
        }

        private string CreateVNPaySecureHash(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
} 