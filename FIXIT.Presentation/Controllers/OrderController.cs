namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController(IServiceManager serviceManager) : ControllerBase
{
    [Authorize(Roles = "Provider")]
    [HttpGet("ByProviderId/{providerId}")]
    public async Task<IActionResult> GetOrdersByProviderId(string providerId)
    {
        var result = await serviceManager.orderService.GetOrdersByProviderId(providerId);
        
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("ByCustomerId/{CustomerId}")]
    public async Task<IActionResult> GetOrdersByCustomerId(string CustomerId)
    {
        var result = await serviceManager.orderService.GetOrdersByCustomerId(CustomerId);

        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDTO order)
    {
        var result = await serviceManager.orderService.CreateOrder(order);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Customer")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var result = await serviceManager.orderService.DeleteOrder(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
