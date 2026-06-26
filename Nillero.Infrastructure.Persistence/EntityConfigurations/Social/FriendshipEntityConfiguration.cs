using Nillero.Core.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nillero.Infrastructure.Persistence.EntityConfigurations.Social
{
    public class FriendshipEntityConfiguration : IEntityTypeConfiguration<Friendship>
    {
        public void Configure(EntityTypeBuilder<Friendship> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("Friendships");

            builder.Property(x => x.CreatedAt).IsRequired();

            // FKs a Identity_Users 
            builder.Property(x => x.User1Id).IsRequired().HasMaxLength(255); 
            builder.Property(x => x.User2Id).IsRequired().HasMaxLength(255); 

            builder.HasIndex(x => new { x.User1Id, x.User2Id })
                .IsUnique()
                .HasDatabaseName("IX_Friendships_User1Id_User2Id");

            builder.HasIndex(x => x.User1Id);
            builder.HasIndex(x => x.User2Id);
        }
    }
}
