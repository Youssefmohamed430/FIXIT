
namespace FIXIT.Presentation.Hubs;

public class ChatHub : Hub
{
    public async Task SendMsg(string id,string msg)
    {

        await Clients.Caller.SendAsync(msg);
    }
}
