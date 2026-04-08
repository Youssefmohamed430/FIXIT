using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace FIXIT.Application.Services;

public class PayMobService : IPaymentGateway
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PayMobService> _logger;
    private readonly string _apiKey;
    private readonly int _integrationId;
    private readonly int _iframeId;
    private readonly string _hmacSecretKey;
    private readonly string _baseUrl = "https://accept.paymob.com/api/";
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceManager serviceManager;
    public PaymentWay paymentWay => PaymentWay.Paymob;


    public PayMobService(
        IHttpClientFactory httpClientFactory, ILogger<PayMobService> logger
        ,IUnitOfWork unitOfWork,IServiceManager _serviceManager)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        serviceManager = _serviceManager;

        this._unitOfWork = unitOfWork;

        // قراءة المتغيرات من Environment Variables
        _apiKey = Environment.GetEnvironmentVariable("PAYMOP_API_KEY") ?? "ZXlKaGJHY2lPaUpJVXpVeE1pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SndjbTltYVd4bFgzQnJJam94TVRNd01UZ3NJbU5zWVhOeklqb2lUV1Z5WTJoaGJuUWlMQ0p1WVcxbElqb2lhVzVwZEdsaGJDSjkuazJvdExPbXNZajFQM1FFNTBfeU1mWVBZS0U3S3VuTEpKMThRRkgzR1V0a3dXZG5wNG5kQlc3eDc1WGpOcHNyUTV0ckVPRzZlX2VkdG9jVjJDcHpzc2c=";

        var integrationIdStr = Environment.GetEnvironmentVariable("PAYMOP_INTEGRATION_ID") ?? "4896849";
        _integrationId = int.Parse(integrationIdStr);

        var iframeIdStr = Environment.GetEnvironmentVariable("PAYMOP_IFRAME_ID") ?? "897502";
        _iframeId = int.Parse(iframeIdStr);

        _hmacSecretKey = Environment.GetEnvironmentVariable("HMAC_SECRET_KEY") ?? "0DC8EF3D0DAAB2C53EC6DDD2BEA8EDD0";

        _logger.LogInformation("PayMobService initialized with Integration ID: {IntegrationId}, IFrame ID: {IFrameId}",
            _integrationId, _iframeId);
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_baseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    // 1) AUTH → GET TOKEN
    public async Task<string> AuthenticateAsync()
    {
        try
        {
            _logger.LogInformation("Starting PayMob authentication...");

            using var client = CreateClient();

            var body = new { api_key = _apiKey };

            var response = await client.PostAsJsonAsync("auth/tokens", body);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("PayMob authentication failed. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Authentication failed: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            if (result == null || string.IsNullOrEmpty(result.token))
            {
                _logger.LogError("PayMob returned null or empty token");
                throw new InvalidOperationException("Invalid authentication response");
            }

            _logger.LogInformation("PayMob authentication successful. Token length: {Length}", result.token.Length);
            return result.token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PayMob authentication");
            throw;
        }
    }

    // 2) CREATE ORDER
    public async Task<int> CreateOrderAsync(string token, int amountCents)
    {
        try
        {
            _logger.LogInformation("Creating PayMob order for amount: {Amount} cents", amountCents);

            using var client = CreateClient();

            var body = new
            {
                auth_token = token,
                delivery_needed = false,
                amount_cents = amountCents * 100,
                currency = "EGP",
                items = Array.Empty<object>()
            };

            var response = await client.PostAsJsonAsync("ecommerce/orders", body);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Order creation failed. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Order creation failed: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<OrderResponse>();

            if (result == null || result.id == 0)
            {
                _logger.LogError("PayMob returned invalid order ID");
                throw new InvalidOperationException("Invalid order response");
            }

            _logger.LogInformation("Order created successfully. Order ID: {OrderId}", result.id);
            return result.id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayMob order");
            throw;
        }
    }

    // 3) GET PAYMENT KEY
    public async Task<string> GetPaymentKeyAsync(string token, int orderId, int amountCents,string passengerid)
    {
        try
        {
            _logger.LogInformation("Getting payment key for order: {OrderId}, amount: {Amount}", orderId, amountCents);

            using var client = CreateClient();

            var customer = await serviceManager._walletService.GetWalletByUserId(passengerid);

            var body = new
            {
                auth_token = token,
                amount_cents = amountCents * 100,
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    apartment = passengerid,
                    email = customer.Value.email,
                    floor = "NA",
                    first_name = customer.Value.Name,
                    street = "NA",
                    building = "NA",
                    phone_number = "01000000000",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "Cairo",
                    country = "EG",
                    last_name = "Doe",
                    state = "Cairo"
                },
                currency = "EGP",
                integration_id = _integrationId
            };

            var response = await client.PostAsJsonAsync("acceptance/payment_keys", body);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Payment key generation failed. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Payment key generation failed: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<PaymentKeyResponse>();

            if (result == null || string.IsNullOrEmpty(result.token))
            {
                _logger.LogError("PayMob returned invalid payment key");
                throw new InvalidOperationException("Invalid payment key response");
            }

            _logger.LogInformation("Payment key generated successfully. Key length: {Length}", result.token.Length);
            return result.token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment key");
            throw;
        }
    }

    // 4) GET IFRAME URL
    public string GetIframeUrl(string paymentKey)
    {
        var url = $"https://accept.paymob.com/api/acceptance/iframes/{_iframeId}?payment_token={paymentKey}";
        _logger.LogInformation("Generated iframe URL with IFrame ID: {IFrameId}", _iframeId);
        return url;
    }

    // العملية الكاملة
    public async Task<string> Pay(int amountCents,string passengerid)
    {
        try
        {
            _logger.LogInformation("=== Starting payment process for {Amount} cents ===", amountCents);

            var token = await AuthenticateAsync();
            _logger.LogInformation("Step 1/3: Authentication completed");

            var orderId = await CreateOrderAsync(token, amountCents);
            _logger.LogInformation("Step 2/3: Order created with ID: {OrderId}", orderId);

            var paymentKey = await GetPaymentKeyAsync(token, orderId, amountCents, passengerid);
            _logger.LogInformation("Step 3/3: Payment key generated");

            var iframeUrl = GetIframeUrl(paymentKey);
            _logger.LogInformation("=== Payment process completed successfully ===");
            _logger.LogInformation("IFrame URL: {Url}", iframeUrl);

            return iframeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== Payment process failed ===");
            throw;
        }
    }

    // التحقق من Callback
    public async Task<bool> RecieveCallback(object payload, Dictionary<string, string> headers)
    {
        try
        {
            var paymobPayload = (PaymobCallback)payload;

            if (paymobPayload.obj.data?.message != null)
            {
                _logger.LogWarning("PayMob Error Message: {Message}", paymobPayload.obj.data.message);
                _logger.LogWarning("TXN Response Code: {Code}", paymobPayload.obj.data.txn_response_code);
            }

            // بناء الـ data string للتحقق من HMAC
            var dataString = $"{paymobPayload.obj.amount_cents}" +
                             $"{paymobPayload.obj.created_at}" +
                             $"{paymobPayload.obj.currency}" +
                             $"{paymobPayload.obj.error_occured.ToString().ToLower()}" +
                             $"{paymobPayload.obj.has_parent_transaction.ToString().ToLower()}" +
                             $"{paymobPayload.obj.id}" +
                             $"{paymobPayload.obj.integration_id}" +
                             $"{paymobPayload.obj.is_3d_secure.ToString().ToLower()}" +
                             $"{paymobPayload.obj.is_auth.ToString().ToLower()}" +
                             $"{paymobPayload.obj.is_capture.ToString().ToLower()}" +
                             $"{paymobPayload.obj.is_refunded.ToString().ToLower()}" +
                             $"{paymobPayload.obj.is_standalone_payment.ToString().ToLower()}" +
                             $"{paymobPayload.obj.is_voided.ToString().ToLower()}" +
                             $"{paymobPayload.obj.order.id}" +
                             $"{paymobPayload.obj.owner}" +
                             $"{paymobPayload.obj.pending.ToString().ToLower()}" +
                             $"{paymobPayload.obj.source_data.pan}" +
                             $"{paymobPayload.obj.source_data.sub_type}" +
                             $"{paymobPayload.obj.source_data.type}" +
                             $"{paymobPayload.obj.success.ToString().ToLower()}";

            // حساب HMAC
            var hmacHeader = headers["hmac"];
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_hmacSecretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
            string calculatedHmac = BitConverter.ToString(hash).Replace("-", "").ToLower();

            // مقارنة HMAC
            if (calculatedHmac != hmacHeader?.ToLower())
            {
                _logger.LogWarning("HMAC verification failed. Expected: {Expected}, Got: {Got}",
                    calculatedHmac, hmacHeader);
                return false;
            }

            _logger.LogInformation("HMAC verified successfully. Payment success: {Success}",
                paymobPayload.obj.success);

            return Convert.ToBoolean(paymobPayload.obj.success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayMob callback");
            return false;
        }
    }
    public Task<decimal> ExtractAmountAsync(object payload)
    {
        var paymobPayload = (PaymobCallback)payload;
        return Task.FromResult(
            Convert.ToDecimal(paymobPayload.obj.amount_cents) / 100);
    }

    public Task<string> ExtractCustomerIdAsync(object payload)
    {
        var paymobPayload = (PaymobCallback)payload;
        return Task.FromResult(
            paymobPayload.obj.payment_key_claims.billing_data.apartment);
    }
}

// DTOs
public class AuthResponse
{
    public string token { get; set; }
}

public class OrderResponse
{
    public int id { get; set; }
}

public class PaymentKeyResponse
{
    public string token { get; set; }
}