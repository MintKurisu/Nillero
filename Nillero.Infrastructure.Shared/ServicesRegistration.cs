using Nillero.Core.Application.Interfaces.Email;
using Nillero.Core.Domain.Settings;
using Nillero.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nillero.Core.Application
{
    public static class ServicesRegistration
    {
        //Extension method - decorator pattern
        public static void AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Configurations
            services.Configure<MailSettings>(config.GetSection("MailSettings"));

            #endregion

            #region Services IOC

            services.AddScoped<IEmailService, EmailService>();

            #endregion

        }
    }
}
