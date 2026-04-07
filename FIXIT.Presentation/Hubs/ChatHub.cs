
namespace FIXIT.Presentation.Hubs;

public class ChatHub(IServiceManager serviceManager) : Hub
{
    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
    }

    public async Task SendMsg(MessageDto messageDto)
    {
        var result = await serviceManager.ChatService.SendMsg(messageDto);

        await Clients.Group(messageDto.ChatId.ToString())
            .SendAsync("ReceiveMessage", result.Value);
    }
}