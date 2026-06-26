using Nillero.Core.Domain.Interfaces.Base;
using Nillero.Core.Domain.Interfaces.Social;
using Nillero.Infrastructure.Persistence.Contexts;
using Nillero.Infrastructure.Persistence.Repositories.Base;
using Nillero.Infrastructure.Persistence.Repositories.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nillero.Infrastructure.Persistence
{
    public static class ServicesRegistration
    {
        //Extension method - decorator pattern
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<NilleroAppContext>(options =>
                {
                    options.UseInMemoryDatabase("NilleroInMemoryDb");
                });
            }
            else
            {
                var connectionString = config.GetConnectionString("DefaultConnection");
                services.AddDbContext<NilleroAppContext>(
                    options =>
                    {
                        options.EnableSensitiveDataLogging();
                        options.UseNpgsql(
                            connectionString,
                            npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                                typeof(NilleroAppContext).Assembly.FullName)
                        );
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                );
            }

            #endregion

            #region Repositories IOC

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            services.AddScoped<IPostReactionRepository, PostReactionRepository>();

            #endregion
        }

    }
 
}
