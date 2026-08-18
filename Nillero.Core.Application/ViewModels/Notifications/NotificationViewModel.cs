using Nillero.Core.Domain.Common.Enum;

namespace Nillero.Core.Application.ViewModels.Notifications
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string ActorUsername { get; set; } = string.Empty;
        public string ActorAvatarUrl { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }

        public string GetTimeAgo()
        {
            var timeSpan = DateTime.UtcNow - CreatedAt.ToUniversalTime();

            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays}d ago";

            return CreatedAt.ToString("MMM dd, yyyy");
        }
    }
}
