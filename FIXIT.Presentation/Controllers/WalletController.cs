
using FIXIT.Application.IServices;
using Stripe;

namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class WalletController(IServiceManager serviceManager,ILogger<WalletController> logger) : ControllerBase
{
    [HttpGet("{Id}")]
    [Authorize]
    public async Task<IActionResult> GetWalletByUserId(string Id)
    {
        var result = await serviceManager._walletService.GetWalletByUserId(Id);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpPut("{amount}/{Customerid}/{paymentWay}")]
    [ServiceFilter(typeof(IdempotencyKeyFilter))]
    public async Task<IActionResult> ChargeWallet([FromHeader(Name = "Idempotency-Key")] string Key,double amount, string Customerid,PaymentWay paymentWay)
    {
        try
        {
            var iframeUrl = await serviceManager._walletService
                .ChargeWallet(amount, Customerid,paymentWay);

            return Ok(new { iframeUrl = iframeUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("Withdraw")]
    [Authorize]

    public async Task<IActionResult> WithdrawFromWallet([FromBody] WithdrawDTO withdrawDTO)
    {
        try
        {
            var result = await serviceManager._walletService.Withdraw(withdrawDTO);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> RecieveCallback()
    {
        // Fawaterak Webhook
        if (Request.Headers.ContainsKey("Authorization"))
        {
            try
            {
                var payload = await Request.ReadFromJsonAsync<WebHookModel>();
                logger.LogInformation("Received Fawaterak callback: {@Payload}", payload);

                var headers = new Dictionary<string, string>
                {   
                    { "Authorization", Request.Headers["Authorization"].ToString() }
                };

                var result = await serviceManager._walletService
                    .RecieveCallback(payload, headers, PaymentWay.Fawaterek);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Fawaterak callback");
                return BadRequest();
            }
        }
        // Paymob Webhook
        else if (Request.Query.ContainsKey("hmac"))
        {
            try
            {
                var payload = await Request.ReadFromJsonAsync<PaymobCallback>();
                logger.LogInformation("Received Paymob callback: {@Payload}", payload);

                var headers = new Dictionary<string, string>
            {
                { "hmac", Request.Query["hmac"].FirstOrDefault() ?? "" }
            };

                var result = await serviceManager._walletService
                    .RecieveCallback(payload, headers, PaymentWay.Paymob);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing Paymob callback");
                return BadRequest();
            }
        }

        return BadRequest("Unknown payment provider");
    }
}
