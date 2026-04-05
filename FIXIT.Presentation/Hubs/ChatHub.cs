
namespace FIXIT.Presentation.Hubs;

public class ChatHub : Hub
{
    public async Task<string> SendMsg(string id,string msg)
    {

        await Clients.Caller.SendAsync(msg);
    }
}
