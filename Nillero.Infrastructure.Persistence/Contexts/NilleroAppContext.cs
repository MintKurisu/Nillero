using Nillero.Core.Domain.Entities.Social;
using Nillero.Infrastructure.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Nillero.Infrastructure.Persistence.Contexts
{
    public class NilleroAppContext : DbContext 
    {
        public NilleroAppContext(DbContextOptions<NilleroAppContext> options) : base(options) { }

        // Social
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<ApplicationUser>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
