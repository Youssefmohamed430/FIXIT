namespace FIXIT.Application.IServices;

public interface IChatService
{
    Task<Result<ChatDTO>> GetOrCreateChat(CreateChatDTO chatDTO);
    Task<Result<List<ChatDTO>>> GetAllChats(string userId);
    Task<Result<object>> DeleteChat(int ChatId);
    Task<Result<MessageDto>> SendMsg(MessageDto chatDTO);
}
