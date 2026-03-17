namespace FIXIT.Application.IServices;

public interface INotifService
{
    Task<Result<List<NotifDTO>>> GetNotifsByUserId(string userId);
    Task<Result<NotifDTO>> CreateNotif(NotifDTO notifDTO);
    Task<Result<NotifDTO>> MarkNotifAsRead(int notifid);
}
