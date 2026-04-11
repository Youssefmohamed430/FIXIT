namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class NotificationController(IServiceManager serviceManager) : ControllerBase
{
    [HttpGet("{Id}")]
    public async Task<IActionResult> GetNotifs(string Id)
    {
        var result = await serviceManager.notifService.GetNotifsByUserId(Id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [HttpPut("MarkasRead/{notifid}")]
    public async Task<IActionResult> MarkNotifAsRead(int notifid)
    {
        var result = await serviceManager.notifService.MarkNotifAsRead(notifid);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
