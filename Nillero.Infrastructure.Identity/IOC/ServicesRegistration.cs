using Nillero.Core.Application.Interfaces.User;
using Nillero.Infrastructure.Identity.Contexts;
using Nillero.Infrastructure.Identity.Entities;
using Nillero.Infrastructure.Identity.Seeds;
using Nillero.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nillero.Infrastructure.Identity.IOC
{
    public static class ServicesRegistration
    {
        public static void AddIdentityInfrastructureLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            ConfigureDatabase(services, config);

            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireNonAlphanumeric = true; // true for testing, fix in prod 
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;

                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opt.Lockout.MaxFailedAccessAttempts = 5;

                opt.User.RequireUniqueEmail = true;

                opt.SignIn.RequireConfirmedEmail = false; // false for testing, fix in prod 
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(12);
            });

            services.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = "/Login/Index";
                opt.AccessDeniedPath = "/Login/AccessDenied";
                opt.ExpireTimeSpan = TimeSpan.FromHours(3);
                opt.SlidingExpiration = true; 
            });

            #region Services
            services.AddScoped<IAccountServicesForWebApp, AccountServicesForWebApp>();
            #endregion
        }

        public static async Task RunIdentitySeedAsync(this IServiceProvider service) 
        {
            using var scoped = service.CreateScope();
            var servicesProvider = scoped.ServiceProvider;

            var userManager = servicesProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = servicesProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await DefaultRoles.SeedAsync(roleManager);
            await DefaultUser.SeedAsync(userManager);
        }

        private static void ConfigureDatabase(IServiceCollection services, IConfiguration config)
        {
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<IdentityContext>(options =>
                {
                    options.UseInMemoryDatabase("NilleroIdentityDb");
                });
            }
            else
            {
                var connectionString = config.GetConnectionString("DefaultConnection");

                services.AddDbContext<IdentityContext>(
                    options =>
                    {
                        options.EnableSensitiveDataLogging();
                        options.UseNpgsql(
                            connectionString,
                            npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)
                        );
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                );
            }
        }
    }
}
