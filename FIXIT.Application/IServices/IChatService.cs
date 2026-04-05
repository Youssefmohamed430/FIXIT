namespace FIXIT.Application.IServices;

public interface IChatService
{
    Task<Result<ChatDTO>> GetChat(string id,string ParticpentId);
    //Task<Result<ChatDTO>> GetAllChats(string id,string ParticpentId);
    Task<Result<ChatDTO>> CreateChat(ChatDTO chatDTO);
    Task<Result<object>> DeleteChat(int ChatId);
    Task<Result<ChatDTO>> SendMsg(CreateChatDTO chatDTO);
}
