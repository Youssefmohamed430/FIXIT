
namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class WalletController(IServiceManager serviceManager,ILogger<WalletController> logger) : ControllerBase
{
    [HttpGet("Id")]
    [Authorize]
    public async Task<IActionResult> GetWalletByUserId(string Id)
    {
        var result = await serviceManager._walletService.GetWalletByUserId(Id);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
    [HttpPut("{amount}/{Customerid}")]
    public async Task<IActionResult> ChargeWallet(double amount, string Customerid)
    {
        try
        {
            var iframeUrl = await serviceManager._walletService
                .ChargeWallet(amount, Customerid);

            return Ok(new { iframeUrl = iframeUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymobCallback([FromBody] PaymobCallback payload)
    {
        Result<object> result = null!;
        try
        {
            logger.LogInformation("Received Paymob callback: {@Payload}", payload);

            var hmacHeader = Request.Query["hmac"].FirstOrDefault();

            result = await serviceManager._walletService.PaymobCallback(payload, hmacHeader);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging Paymob callback");
            result = Result<object>.Failure(new Error("PaymobCallbackError", "An error occurred while processing the Paymob callback."));
        }
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
