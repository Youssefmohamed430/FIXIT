
namespace FIXIT.Application.Servicces;

public class NotifService(IUnitOfWork unitOfWork) : INotifService
{
    public async Task<Result<NotifDTO>> CreateNotif(NotifDTO notifDTO)
    {
        var notif = new Notification
        {
            Message = notifDTO.Message,
            Date = notifDTO.Date,
        };

        await unitOfWork.GetRepository<Notification>().AddAsync(notif);
        await unitOfWork.SaveAsync();

        var userNotif = new UserNotification
        {
            NotifId = notif.NotifId,
            UserId = notifDTO.UserId
        };

        await unitOfWork.GetRepository<UserNotification>().AddAsync(userNotif);
        await unitOfWork.SaveAsync();

        return Result<NotifDTO>.Success(notifDTO);
    }

    public async Task<Result<List<NotifDTO>>> GetNotifsByUserId(string userId)
    {
        var notifs = await unitOfWork.GetRepository<UserNotification>()
            .FindAllAsync<NotifDTO>(n => n.UserId == userId);

        if(notifs == null)
            return Result<List<NotifDTO>>.Failure(new Error("Notifs.NotFound.UserId", "Notifications for this user not found"));

        return Result<List<NotifDTO>>.Success(notifs.ToList());
    }

    public async Task<Result<NotifDTO>> MarkNotifAsRead(int notifid)
    {
        var notif = await unitOfWork.GetRepository<UserNotification>()
            .FindAsync(n => n.NotifId == notifid);

        if(notif == null)
            return Result<NotifDTO>.Failure(new Error("Notif.NotFound", "Notification not found"));

        notif.IsRead = true;
        await unitOfWork.GetRepository<UserNotification>().UpdateAsync(notif);
        await unitOfWork.SaveAsync();

        var notifDTO = new NotifDTO
        {
            Message = notif.Notif?.Message ?? string.Empty,
            Date = notif.Notif?.Date ?? DateTime.MinValue,
            UserId = notif.UserId,
            IsRead = notif.IsRead
        };

        return Result<NotifDTO>.Success(notifDTO);
    }

    //public async Task SendNotification<T>(CreateOfferDTO offer)
    //{
    //    JobPost jobPost = unitOfWork.GetRepository<JobPost>().Find(j => j.Id == offer.JobPostId);

    //    await serviceManager.notifService.CreateNotif(new NotifDTO
    //    {
    //        UserId = jobPost.CustomerId,
    //        Message = $"New offer created for your job post with price {offer.Price}.",
    //    });
    //}
}
