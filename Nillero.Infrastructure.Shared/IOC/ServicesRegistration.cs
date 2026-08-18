using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nillero.Core.Application.Interfaces.Email;
using Nillero.Core.Application.Interfaces.Storage;
using Nillero.Core.Domain.Settings;
using Nillero.Infrastructure.Shared.Services.Email;
using Nillero.Infrastructure.Shared.Services.Storage;

namespace Nillero.Infrastructure.Shared.IOC
{
    public static class ServicesRegistration
    {
        public static async Task AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Configurations
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            #endregion

            #region Supabase
            var supabaseUrl = config["Supabase:Url"]!;
            var supabaseKey = config["Supabase:SecretKey"]!;

            var supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = false
            });

            await supabaseClient.InitializeAsync();
            services.AddSingleton(supabaseClient);
            #endregion

            #region Services IOC
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IStorageService, SupabaseStorageService>();
            #endregion

        }
    }
}
