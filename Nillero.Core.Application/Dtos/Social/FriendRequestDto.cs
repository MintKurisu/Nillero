using Nillero.Core.Domain.Common.Enum.Social;

namespace Nillero.Core.Application.Dtos.Social
{
    public class FriendRequestDto
    {
        public int Id { get; set; }
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }
        public required FriendRequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; } 
    }
}
