namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class WalletController(IServiceManager serviceManager) : ControllerBase
{
    [HttpGet("Id")]
    [Authorize]
    public async Task<IActionResult> GetWalletByUserId(string Id)
    {
        var result = await serviceManager._walletService.GetWalletByUserId(Id);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
