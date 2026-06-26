using Nillero.Core.Domain.Entities.Social;
using Nillero.Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nillero.Infrastructure.Persistence.EntityConfigurations.Social
{
    public class FriendRequestEntityConfiguration : IEntityTypeConfiguration<FriendRequest>
    {
        public void Configure(EntityTypeBuilder<FriendRequest> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("FriendRequests");

            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();

            builder.HasIndex(x => x.Status);

            builder.Property(x => x.SenderId).IsRequired().HasMaxLength(255);
            builder.Property(x => x.ReceiverId).IsRequired().HasMaxLength(255);

            builder.HasIndex(x => x.SenderId);
            builder.HasIndex(x => x.ReceiverId);
        }
    }
}
