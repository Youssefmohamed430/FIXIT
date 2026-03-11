
namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class EscrowPaymentController(IServiceManager serviceManager) : ControllerBase
{
    [Authorize(Roles = "Customer")]
    [HttpPost("AcceptOrder/{orderId}")]
    [ServiceFilter(typeof(IdempotencyKeyFilter))]
    public async Task<IActionResult> AcceptOrder(int orderId, [FromHeader(Name = "Idempotency-Key")] string Key)
    {
        var result = await serviceManager.escrowPaymentService.AcceptOrder(orderId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("CancelOrder/{orderId}")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var result = await serviceManager.escrowPaymentService.CancelOrder(orderId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPost("ChangeWorkOrderStatus/{orderId}/{newStatus}")]
    public async Task<IActionResult> ChangeWorkOrderStatus(int orderId,WorkStatus newStatus)
    {
        var result = await serviceManager.escrowPaymentService.ChangeWorkOrderStatus(orderId, newStatus);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
