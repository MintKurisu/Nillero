using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nillero.Core.Application.Dtos.Notifications;
using Nillero.Core.Application.Interfaces.Notifications;
using Nillero.Core.Application.Services.Base;
using Nillero.Core.Domain.Entities.Notifications;
using Nillero.Core.Domain.Interfaces.Notifications;

namespace Nillero.Core.Application.Services.Notifications
{
    public class NotificationService : GenericService<Notification, NotificationDto>, INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public NotificationService(
            INotificationRepository notificationRepository,
            IMapper mapper) : base(notificationRepository, mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<List<NotificationDto>> GetForUserAsync(string userId)
        {
            try
            {
                var query = _notificationRepository.GetAllQuery();
                var notifications = await query
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
                return _mapper.Map<List<NotificationDto>>(notifications);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetForUserAsync: {ex.Message}");
                return new List<NotificationDto>();
            }
        }

        public async Task<bool> MarkAsReadAsync(int id, string userId)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null || notification.UserId != userId) return false;
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(id, notification);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MarkAsReadAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            try
            {
                var query = _notificationRepository.GetAllQuery();
                var unreadNotifications = await query
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    await _notificationRepository.UpdateAsync(notification.Id, notification);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MarkAllAsReadAsync: {ex.Message}");
                return false;
            }
        }
    }
}
