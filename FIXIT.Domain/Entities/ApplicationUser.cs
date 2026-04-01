

namespace FIXIT.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public required string Name { get; set; }
    public required Point Location { get; set; }
    public ImgPath? Img { get; set; }
    public bool IsDeleted { get; set; } = false;
    public Customer? Customer { get; set; }
    public ServiceProvider? ServiceProvider { get; set; }
    public List<UserNotification>? UserNotifications { get; set; }
    public Wallet? Wallet { get; set; }
    public List<ChatParticipant>? ChatParticipants { get; set; }
    public List<ChatMessage>? ChatMessages { get; set; }
    public List<RefreshToken>? refreshTokens { get; set; }

    public void UpdateFrom(
        string? name,
        string? userName,
        string? phone,
        string? email,
        double? longitude,
        double? latitude,
        string? imgPath)
    {
        if (!string.IsNullOrEmpty(name))
            Name = name;

        if (!string.IsNullOrEmpty(userName))
            UserName = userName;

        if (!string.IsNullOrEmpty(phone))
            PhoneNumber = phone;

        if (!string.IsNullOrEmpty(email))
            Email = email;

        if (longitude.HasValue && latitude.HasValue)
            Location = new Point(longitude.Value, latitude.Value) { SRID = 4326 };

        if (!string.IsNullOrEmpty(imgPath))
            Img = ImgPath.Create(imgPath);
    }
}
