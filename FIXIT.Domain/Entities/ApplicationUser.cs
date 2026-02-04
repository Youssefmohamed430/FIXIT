using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace FIXIT.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required string Name { get; set; }
        public required Point Location { get; set; }
        public ImgPath? Img { get; private set; }
        public bool IsDeleted { get; set; } = false;
        public Customer? Customer { get; set; }
        public ServiceProvider? ServiceProvider { get; set; }
        public List<UserNotification>? UserNotifications { get; set; }
        public Wallet? Wallet { get; set; }
        public List<ChatParticipant>? ChatParticipants { get; set; }
        public List<ChatMessage>? ChatMessages { get; set; }
        public List<RefreshToken>? refreshTokens { get; set; }
    }
}
