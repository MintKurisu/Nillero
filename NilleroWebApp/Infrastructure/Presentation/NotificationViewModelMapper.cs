using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Nillero.Core.Application.Dtos.Notifications;
using Nillero.Core.Application.Interfaces.Presentation.Mappers;
using Nillero.Core.Application.ViewModels.Notifications;
using Nillero.Core.Domain.Common.Enum;
using Nillero.Infrastructure.Identity.Entities;

namespace NilleroWebApp.Infrastructure.Presentation
{
    public class NotificationViewModelMapper : INotificationViewModelMapper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public NotificationViewModelMapper(
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<List<NotificationViewModel>> MapAsync(
            List<NotificationDto> notifications)
        {
            var result = new List<NotificationViewModel>();

            foreach (var notification in notifications)
            {
                result.Add(await MapNotificationAsync(notification));
            }

            return result;
        }

        private async Task<NotificationViewModel> MapNotificationAsync(
            NotificationDto dto)
        {
            var vm = _mapper.Map<NotificationViewModel>(dto);

            if (!string.IsNullOrWhiteSpace(dto.ActorUserId))
            {
                var actor = await _userManager.FindByIdAsync(dto.ActorUserId);

                if (actor != null)
                {
                    vm.ActorUsername = actor.UserName ?? "";

                    vm.ActorAvatarUrl = actor.ProfilePicturePath;

                    vm.Message = dto.Type switch
                    {
                        NotificationType.Comment =>
                            $"{actor.UserName} commented on your post.",

                        NotificationType.Reply =>
                            $"{actor.UserName} replied to your comment.",

                        NotificationType.Like =>
                            $"{actor.UserName} liked your post.",

                        NotificationType.FriendRequest =>
                            $"{actor.UserName} sent you a friend request.",

                        NotificationType.FriendAccepted =>
                            $"{actor.UserName} accepted your friend request.",

                        _ =>
                            "New notification."
                    };
                }
            }

            return vm;
        }
    }
}
