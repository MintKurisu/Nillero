using Nillero.Core.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nillero.Infrastructure.Persistence.EntityConfigurations.Social
{
    public class PostEntityConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("Posts");

            builder.Property(x => x.Content).IsRequired().HasMaxLength(5000);
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.MediaPath).HasMaxLength(500);
            builder.Property(x => x.YouTubeUrl).HasMaxLength(500);
            builder.Property(x => x.CreatedAt).IsRequired();

            // FK a Identity_Users 
            builder.Property(x => x.UserId).IsRequired().HasMaxLength(255);
            builder.HasIndex(x => x.UserId);

            // Relaciones
            builder.HasMany(x => x.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Reactions)
                .WithOne(r => r.Post)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
