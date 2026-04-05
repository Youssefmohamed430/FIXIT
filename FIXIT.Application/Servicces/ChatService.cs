using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Application.Servicces;

public class ChatService : IChatService
{
    public Task<Result<ChatDTO>> CreateChat(ChatDTO chatDTO)
    {
        throw new NotImplementedException();
    }

    public Task<Result<object>> DeleteChat(int ChatId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ChatDTO>> GetChat(string id, string ParticpentId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<ChatDTO>> SendMsg(CreateChatDTO chatDTO)
    {
        throw new NotImplementedException();
    }
}
