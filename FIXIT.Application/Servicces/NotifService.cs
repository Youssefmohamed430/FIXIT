
using Microsoft.Extensions.Localization;

namespace FIXIT.Application.Servicces;

public class NotifService(IUnitOfWork unitOfWork,IStringLocalizer<NotifService> _localizer) : INotifService
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
            .FindAllAsync<NotifDTO>(n => n.UserId == userId,new string[] { "Notif" });

        if(notifs == null)
            return Result<List<NotifDTO>>.Failure(new Error("Notifs.NotFound.UserId", _localizer["Notif.NotFound.UserId"]));

        return Result<List<NotifDTO>>.Success(notifs.ToList());
    }

    public async Task<Result<NotifDTO>> MarkNotifAsRead(int notifid)
    {
        var notif = await unitOfWork.GetRepository<UserNotification>()
            .FindAsync(n => n.NotifId == notifid);

        if(notif == null)
            return Result<NotifDTO>.Failure(new Error("Notif.NotFound", _localizer["Notif.NotFound"]));

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
    public async Task NotifyCustomerByJobPostId(int jobPostId, string message)
    {
        var customerId = unitOfWork.GetRepository<JobPost>()
            .Find(j => j.Id == jobPostId)?.CustomerId;

        if (customerId is null) return;

        await CreateNotif(new NotifDTO
        {
            UserId = customerId,
            Message = message
        });
    }

    public async Task NotifyProviderByOfferId(int offerId, string message)
    {
        var providerId = unitOfWork.GetRepository<Offer>()
            .Find(o => o.Id == offerId)?.ProviderId;

        if (providerId is null) return;

        await CreateNotif(new NotifDTO
        {
            UserId = providerId,
            Message = message
        });
    }
}
