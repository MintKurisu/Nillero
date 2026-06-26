using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Services.Base;
using Nillero.Core.Domain.Entities.Social;
using Nillero.Core.Domain.Interfaces.Social;
using Microsoft.EntityFrameworkCore;

namespace Nillero.Core.Application.Services.Social
{
    public class FriendshipService : GenericService<Friendship, FriendshipDto>, IFriendshipService
    {
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IMapper _mapper;

        public FriendshipService(
            IFriendshipRepository friendshipRepository,
            IMapper mapper) : base(friendshipRepository, mapper)
        {
            _friendshipRepository = friendshipRepository;
            _mapper = mapper;
        }

        public async Task<List<FriendshipDto>> GetFriendsAsync(string userId)
        {
            try
            {
                var query = _friendshipRepository.GetAllQuery();

                var friendships = await query
                    .Where(f => f.User1Id == userId || f.User2Id == userId)
                    .ToListAsync();

                return _mapper.Map<List<FriendshipDto>>(friendships);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFriendsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetFriendIdsAsync(string userId)
        {
            try
            {
                var query = _friendshipRepository.GetAllQuery();

                var friendships = await query
                    .Where(f => f.User1Id == userId || f.User2Id == userId)
                    .ToListAsync();

                // Return friend ids
                var friendIds = friendships
                    .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
                    .ToList();

                return friendIds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetFriendIdsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RemoveFriendshipAsync(string userId, string friendId)
        {
            try
            {
                var user1Id = string.Compare(userId, friendId) < 0 ? userId : friendId;
                var user2Id = string.Compare(userId, friendId) < 0 ? friendId : userId;

                var query = _friendshipRepository.GetAllQuery();
                var friendship = await query
                    .FirstOrDefaultAsync(f => f.User1Id == user1Id && f.User2Id == user2Id);

                if (friendship == null)
                {
                    Console.WriteLine("Error: Friendship not found");
                    return false;
                }

                await _friendshipRepository.DeleteAsync(friendship.Id);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveFriendshipAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetCommonFriendsCountAsync(string userId1, string userId2)
        {
            try
            {
                // Get friends of userId1
                var friends1Ids = await GetFriendIdsAsync(userId1);

                // Get friends of userId2
                var friends2Ids = await GetFriendIdsAsync(userId2);

                // Count common friends
                var commonFriendsCount = friends1Ids.Intersect(friends2Ids).Count();

                return commonFriendsCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCommonFriendsCountAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AreFriendsAsync(string userId1, string userId2)
        {
            try
            {
                var user1Id = string.Compare(userId1, userId2) < 0 ? userId1 : userId2;
                var user2Id = string.Compare(userId1, userId2) < 0 ? userId2 : userId1;

                var query = _friendshipRepository.GetAllQuery();
                var areFriends = await query
                    .AnyAsync(f => f.User1Id == user1Id && f.User2Id == user2Id);

                return areFriends;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in AreFriendsAsync: {ex.Message}");
                throw;
            }
        }
    }
}

