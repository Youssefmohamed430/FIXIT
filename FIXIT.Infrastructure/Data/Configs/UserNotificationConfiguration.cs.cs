using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIXIT.Infrastructure.Data.Configs
{
    public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
    {
        public void Configure(EntityTypeBuilder<UserNotification> builder)
        {
            builder.HasKey(un => new { un.UserId, un.NotifId });

            builder.HasOne(un => un.Notif)
                   .WithMany(n => n.UserNotifications)
                   .HasForeignKey(n => n.NotifId);

            builder.HasOne(un => un.User)
                   .WithMany(u => u.UserNotifications)
                   .HasForeignKey(n => n.UserId);
        }
    }
}
