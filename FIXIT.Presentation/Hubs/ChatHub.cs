
namespace FIXIT.Presentation.Hubs;

public class ChatHub(IServiceManager serviceManager) : Hub
{
    public async Task SendMsg(MessageDto messageDto)
    {
        var result = await serviceManager.ChatService.SendMsg(messageDto);

        await Clients.User(messageDto.SenderId)
            .SendAsync("ReceiveMessage", result.Value);

        await Clients.User(messageDto.RecieverId)
            .SendAsync("ReceiveMessage", result.Value);
    }
}