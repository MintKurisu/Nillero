using Nillero.Core.Application.Dtos.Notifications;
using Nillero.Core.Application.ViewModels.Notifications;

namespace Nillero.Core.Application.Interfaces.Presentation.Mappers
{
    public interface INotificationViewModelMapper
    {
        Task<List<NotificationViewModel>> MapAsync(List<NotificationDto> notifications);
    }
}
