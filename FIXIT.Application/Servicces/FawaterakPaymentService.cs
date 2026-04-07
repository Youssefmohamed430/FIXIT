using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Http.Headers;
using static FIXIT.Application.DTOs.EInvoiceResponseModel;


namespace FIXIT.Application.Servicces;

public class FawaterakPaymentService : IPaymentGateway
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string ApiKey;
    private readonly string BaseUrl;
    private readonly string ProviderKey;
    private IUnitOfWork unitOfWork;
    private readonly ILogger<FawaterakPaymentService> _logger;

    public PaymentWay paymentWay => PaymentWay.Fawaterek;

    public FawaterakPaymentService(ILogger<FawaterakPaymentService> _logger, IHttpClientFactory httpClientFactory,IUnitOfWork _unitofwork)
    {
        _httpClientFactory = httpClientFactory;
        ApiKey = Environment.GetEnvironmentVariable("Fawaterek_API_KEY")!;
        BaseUrl = "https://staging.fawaterk.com/api/v2";
        ProviderKey = Environment.GetEnvironmentVariable("PROVIDER_KEY")!;
        unitOfWork = _unitofwork;
        this._logger = _logger;
    }

    #region Create EInvoice Link

    public async Task<EInvoiceResponseDataModel?> CreateEInvoiceAsync(string UserId, int amountcents)
    {
        var client = _httpClientFactory.CreateClient();

        var eInvoice = await CreateInvoiceModel(UserId, amountcents);

        var url = $"{BaseUrl}/createInvoiceLink";

        var json = JsonConvert.SerializeObject(eInvoice);

        _logger.LogInformation("🔑 API Key: {Key}", ApiKey);

        _logger.LogInformation("🔥 Sending Request To: {Url}", url);
        _logger.LogInformation("📦 Request Body: {Body}", json);

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.SendAsync(request);

            _logger.LogInformation("📡 Status Code: {StatusCode}", response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 Response Body: {Response}", responseContent);

            if (response.IsSuccessStatusCode)
            {
                var _response = JsonConvert.DeserializeObject<EInvoiceResponseModel>(responseContent);

                _logger.LogInformation("✅ Invoice Created Successfully. InvoiceId: {InvoiceId}", _response?.Data?.InvoiceId);

                return _response!.Data;
            }

            _logger.LogError("❌ Request Failed with Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Exception while sending invoice request");
            throw;
        }
    }

    private async Task<EInvoiceRequestModel> CreateInvoiceModel(string UserId, int amountcents)
    {
        var user = unitOfWork.GetRepository<ApplicationUser>()
                    .Find<UserDTO>(u => u.Id == UserId);

        var eInvoice = new EInvoiceRequestModel
        {
            Currency = "EGP",
            CartTotal = amountcents,
            Customer = new EInvoiceRequestModel.CustomerModel
            {
                FirstName = user.Name,
                LastName = user.UserName,
                Email = user.Email,
                Phone = user.Phone
            },

            RedirectionUrls = new EInvoiceRequestModel.RedirectionUrlsModel
            {
                OnSuccess = "https://youssefmohamed430.github.io/PaymentSuccessPage/",
                OnFailure = "https://dev.fawaterk.com/fail",
                OnPending = "https://dev.fawaterk.com/pending"
            },

            CartItems = new List<EInvoiceRequestModel.CartItemModel>
            {
                new EInvoiceRequestModel.CartItemModel
                {
                    Name = "Charge Wallet",
                    Price = amountcents,
                    Quantity = 1
                }
            }
        };
        return eInvoice;
    }
    #endregion

    #region WebHook Verification
    public bool VerifyWebhook(WebHookModel webHook)
    {
        var generatedHashKey =
            GenerateHashKeyForWebhookVerification(webHook.InvoiceId, webHook.InvoiceKey, webHook.PaymentMethod);
        return generatedHashKey == webHook.HashKey;
    }

    public bool VerifyCancelTransaction(CancelTransactionModel cancelTransaction)
    {
        var generatedHashKey = GenerateHashKeyForCancelTransaction(cancelTransaction.ReferenceId, cancelTransaction.PaymentMethod);
        return generatedHashKey == cancelTransaction.HashKey;
    }

    public bool VerifyApiKeyTransaction(string apiKey)
    {
        return apiKey == ApiKey;
    }
    #endregion

    #region Generate HashKey
    public string GenerateHashKeyForIFrame(string domain)
    {
        var queryParam = $"Domain={domain}&ProviderKey={ProviderKey}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ApiKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryParam));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private string GenerateHashKeyForWebhookVerification(long invoiceId, string invoiceKey, string paymentMethod)
    {
        var queryParam = $"InvoiceId={invoiceId}&InvoiceKey={invoiceKey}&PaymentMethod={paymentMethod}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ApiKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryParam));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private string GenerateHashKeyForCancelTransaction(string referenceId, string paymentMethod)
    {
        var queryParam = $"referenceId={referenceId}&PaymentMethod={paymentMethod}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ApiKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryParam));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private static string NormalizeEmailDots(string email)
    {
        if (string.IsNullOrEmpty(email)) return email;

        int atIndex = email.IndexOf('@');
        if (atIndex < 0) return email;

        string localPart = email.Substring(0, atIndex);
        string domainPart = email.Substring(atIndex);

        localPart = System.Text.RegularExpressions.Regex.Replace(localPart, @"\.+", ".");

        return localPart + domainPart;
    }
    #endregion

    public async Task<string> Pay(int amountCents, string userid)
    {
        var result = await CreateEInvoiceAsync(userid,amountCents);

        return result?.Url!;
    }

    public Task<bool> RecieveCallback(object payload, string hmacHeader)
    {
        throw new NotImplementedException();
    }

    public Task<decimal> ExtractAmountAsync(object payload)
    {
        throw new NotImplementedException();
    }

    public Task<string> ExtractCustomerIdAsync(object payload)
    {
        throw new NotImplementedException();
    }
}