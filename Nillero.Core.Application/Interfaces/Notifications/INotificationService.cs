using Nillero.Core.Application.Dtos.Notifications;
using Nillero.Core.Application.Interfaces.Base;

namespace Nillero.Core.Application.Interfaces.Notifications
{
    public interface INotificationService : IGenericService<NotificationDto>
    {
        Task<List<NotificationDto>> GetForUserAsync(string userId);
        Task<bool> MarkAsReadAsync(int id, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
