
namespace FIXIT.Presentation.Hubs;

public class ChatHub(IServiceManager serviceManager) : Hub
{
    public async Task SendMsg(MessageDto messageDto)
    {
        var result = await serviceManager.ChatService.SendMsg(messageDto);

        await Clients.All.SendAsync("ReceiveMessage", result.Value);
    }
}