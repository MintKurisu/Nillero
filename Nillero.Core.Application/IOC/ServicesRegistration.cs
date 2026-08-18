using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Services.Social;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using Nillero.Core.Application.Interfaces.Notifications;
using Nillero.Core.Application.Services.Notifications;

namespace Nillero.Core.Application.IOC
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Mappings
            services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
            #endregion

            #region FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            #endregion

            #region ServicesIOC
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IFriendRequestService, FriendRequestService>();
            services.AddScoped<IFriendshipService, FriendshipService>();
            services.AddScoped<IPostReactionService, PostReactionService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<INotificationService, NotificationService>();
            #endregion
        }
    }
}
