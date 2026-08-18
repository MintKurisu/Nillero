using Nillero.Core.Domain.Common.Enum;

namespace Nillero.Core.Application.Dtos.Notifications
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string ActorUserId { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
    }
}
