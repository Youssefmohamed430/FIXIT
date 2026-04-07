namespace FIXIT.Presentation.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController(IServiceManager serviceManager) : ControllerBase
{
    [HttpGet("{UserId}")]
    public async Task<IActionResult> GetAllChats(string UserId)
    {
        var result = await serviceManager.ChatService.GetAllChats(UserId);

       return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("GetOrCreateChat")]
    public async Task<IActionResult> GetOrCreateChat([FromBody]CreateChatDTO createChatDTO)
    {
        var result = await serviceManager.ChatService.GetOrCreateChat(createChatDTO);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [HttpDelete("{chatId}")]
    public async Task<IActionResult> DeleteChat(int chatId)
    {
        var result = await serviceManager.ChatService.DeleteChat(chatId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
