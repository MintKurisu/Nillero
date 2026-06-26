using Nillero.Core.Application.Dtos.Social;

namespace Nillero.Core.Application.Interfaces.Social
{
    public interface IFriendshipService
    {
        Task<List<FriendshipDto>> GetFriendsAsync(string userId); 
        Task<bool> RemoveFriendshipAsync(string userId, string friendId);
        Task<int> GetCommonFriendsCountAsync(string userId1, string userId2);
        Task<bool> AreFriendsAsync(string userId1, string userId2);
        Task<List<string>> GetFriendIdsAsync(string userId); 
    }
}
