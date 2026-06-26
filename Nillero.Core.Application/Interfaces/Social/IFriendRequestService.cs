using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Base;

namespace Nillero.Core.Application.Interfaces.Social
{
    public interface IFriendRequestService : IGenericService<FriendRequestDto>
    {
        Task<List<FriendRequestDto>> GetPendingReceivedRequestsAsync(string userId);
        Task<List<FriendRequestDto>> GetSentRequestsAsync(string userId);
        Task<bool> AcceptRequestAsync(int requestId);
        Task<bool> RejectRequestAsync(int requestId);
        Task<bool> CanSendRequestAsync(string senderId, string receiverId);
        Task<int> GetPendingRequestsCountAsync(string userId);

    }
}
