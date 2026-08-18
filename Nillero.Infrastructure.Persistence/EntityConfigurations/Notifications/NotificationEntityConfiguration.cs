using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nillero.Core.Domain.Entities.Notifications;

namespace Nillero.Infrastructure.Persistence.EntityConfigurations.Notifications
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.ActorUserId)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(n => n.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                .IsRequired();

            builder.Property(n => n.Type)
                .IsRequired();

            builder.Property(n => n.UserId)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(n => new { n.UserId, n.IsRead });
        }
    }
}

