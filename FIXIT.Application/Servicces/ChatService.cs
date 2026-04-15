using Microsoft.Extensions.Localization;
using System.Security.AccessControl;

namespace FIXIT.Application.Servicces;

public class ChatService(IUnitOfWork unitOfWork,IStringLocalizer<ChatService> _localizer) : IChatService
{
    public async Task<Result<List<ChatDTO>>> GetAllChats(string userId)
    {
        var chatsQuery = await unitOfWork
            .GetRepository<Chat>()
            .FindAllAsync<ChatDTO>(
                c => c.Participants.Any(p => p.UserId == userId) && !c.IsDeleted,
                new[] { "Participants.User", "Messages" }
            );

        var chats = await chatsQuery.ToListAsync();

        foreach (var chat in chats)
        {
            chat.Participants = chat.Participants
                .Where(p => p.UserId != userId)
                .ToList();

            chat.Messages = chat.Messages?
                .OrderByDescending(m => m.SentAt)
                .Take(1)
                .ToList();
        }

        return Result<List<ChatDTO>>.Success(chats);
    }
    public async Task<Result<ChatDTO>> GetOrCreateChat(CreateChatDTO chatDTO)
    {
        var chatRepo = unitOfWork.GetRepository<Chat>();

        var existingChat = await chatRepo.FindAsync(
            c => c.Participants.Any(p => p.UserId == chatDTO.SenderId) &&
                 c.Participants.Any(p => p.UserId == chatDTO.ReceiverId),
            new[] { "Participants.User", "Messages" }
        );

        if (existingChat != null)
        {
            var existingDto = existingChat.Adapt<ChatDTO>();

            existingDto.Messages = existingDto.Messages?
                .OrderBy(m => m.SentAt)
                .ToList();

            return Result<ChatDTO>.Success(existingDto);
        }

        var chat = await CreateChat(chatDTO);

        return chat;
    }

    private async Task<Result<ChatDTO>> CreateChat(CreateChatDTO CreatedchatDTO)
    {
        var chat = await CreateChatEntity(CreatedchatDTO);

        var createdChat = await unitOfWork
            .GetRepository<Chat>()
            .FindAsync(
                c => c.Id == chat.Id,
                new[] { "Participants.User", "Messages" }
            );

        return Result<ChatDTO>.Success(createdChat.Adapt<ChatDTO>());
    }
    private async Task<Chat> CreateChatEntity(CreateChatDTO chatDTO)
    {
        var chat = new Chat();

        await unitOfWork.GetRepository<Chat>().AddAsync(chat);
        await unitOfWork.SaveAsync();

        await CreateParticipants(chatDTO, chat);

        return chat;
    }


    private async Task CreateParticipants(CreateChatDTO chatDTO, Chat chat)
    {
        var participantOne = new ChatParticipant
        {
            ChatId = chat.Id,
            UserId = chatDTO.SenderId
        };

        var participantTwo = new ChatParticipant
        {
            ChatId = chat.Id,
            UserId = chatDTO.ReceiverId
        };

        await unitOfWork.GetRepository<ChatParticipant>().AddAsync(participantOne);
        await unitOfWork.GetRepository<ChatParticipant>().AddAsync(participantTwo);

        await unitOfWork.SaveAsync();
    }


    public async Task<Result<MessageDto>> SendMsg(MessageDto msgDto)
    {
        var sender = unitOfWork.GetRepository<ApplicationUser>()
            .Find(u => u.Id == msgDto.SenderId);

        var reciever = unitOfWork.GetRepository<ApplicationUser>()
            .Find(u => u.Id == msgDto.RecieverId);

        var msg = msgDto.Adapt<ChatMessage>();

        await unitOfWork.GetRepository<ChatMessage>().AddAsync(msg);
        await unitOfWork.SaveAsync();

        msgDto.Id = msg.Id;
        msgDto.SenderName = sender.Name;
        msgDto.RecieverName = reciever.Name;
        return Result<MessageDto>.Success(msgDto);
    }
    public async Task<Result<object>> DeleteChat(int ChatId)
    {
        var chat = await unitOfWork.GetRepository<Chat>().FindAsync(c => c.Id == ChatId);

        if (chat == null)
            return Result<object>.Failure(new Error(_localizer["Chat.NotFound"]));

        unitOfWork.GetRepository<Chat>().DeleteAsync(chat);
        await unitOfWork.SaveAsync();

        return Result<object>.Success(null);
    }
}
