
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
        if (Request.Headers.ContainsKey("Stripe-Signature"))
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"];

            try
            {
                //var result = serviceManager._walletService.RecieveCallback(json, signature);

                //var stripeEvent = EventUtility.ConstructEvent(
                //    json,
                //    signature,
                //    Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")
                //);

                //if (stripeEvent.Type == "payment_intent.succeeded")
                //{
                //    // TODO: update wallet
                //}

                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Stripe webhook error");
                return BadRequest();
            }
        }
        else if (Request.Query.ContainsKey("hmac"))
        {
            try
            {
                var payload = await Request.ReadFromJsonAsync<PaymobCallback>();

                logger.LogInformation("Received Paymob callback: {@Payload}", payload);

                var hmacHeader = Request.Query["hmac"].FirstOrDefault();

                var result = await serviceManager._walletService
                    .RecieveCallback(payload, hmacHeader, PaymentWay.Paymob);

                return result.IsSuccess ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error logging Paymob callback");
                return BadRequest();
            }
        }

        return BadRequest("Unknown payment provider");
    }
}
