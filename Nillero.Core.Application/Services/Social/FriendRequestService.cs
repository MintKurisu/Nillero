using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Services.Base;
using Nillero.Core.Domain.Common.Enum.Social;
using Nillero.Core.Domain.Entities.Social;
using Nillero.Core.Domain.Interfaces.Social;
using Microsoft.EntityFrameworkCore;

namespace Nillero.Core.Application.Services.Social
{
    public class FriendRequestService : GenericService<FriendRequest, FriendRequestDto>, IFriendRequestService
    {
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IMapper _mapper;

        public FriendRequestService(
            IFriendRequestRepository friendRequestRepository,
            IFriendshipRepository friendshipRepository,
            IMapper mapper) : base(friendRequestRepository, mapper)
        {
            _friendRequestRepository = friendRequestRepository;
            _friendshipRepository = friendshipRepository;
            _mapper = mapper;
        }

        public async Task<List<FriendRequestDto>> GetPendingReceivedRequestsAsync(string userId)
        {
            try
            {
                var query = _friendRequestRepository.GetAllQuery();

                var requests = await query
                    .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                    .OrderByDescending(fr => fr.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<FriendRequestDto>>(requests);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPendingReceivedRequestsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<FriendRequestDto>> GetSentRequestsAsync(string userId)
        {
            try
            {
                var query = _friendRequestRepository.GetAllQuery();

                var requests = await query
                    .Where(fr => fr.SenderId == userId)
                    .OrderByDescending(fr => fr.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<FriendRequestDto>>(requests);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSentRequestsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AcceptRequestAsync(int requestId)
        {
            try
            {
                var query = _friendRequestRepository.GetAllQuery();
                var request = await query.FirstOrDefaultAsync(fr => fr.Id == requestId);

                if (request == null || request.Status != FriendRequestStatus.Pending)
                {
                    Console.WriteLine("Request not found or already processed");
                    throw new InvalidOperationException("The request does not exist or has already been processed");
                }

                request.Status = FriendRequestStatus.Accepted;
                request.RespondedAt = DateTime.UtcNow;
                await _friendRequestRepository.UpdateAsync(requestId, request);

                var user1Id = string.Compare(request.SenderId, request.ReceiverId) < 0
                    ? request.SenderId
                    : request.ReceiverId;
                var user2Id = string.Compare(request.SenderId, request.ReceiverId) < 0
                    ? request.ReceiverId
                    : request.SenderId;

                var friendship = new Friendship
                {
                    User1Id = user1Id,
                    User2Id = user2Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _friendshipRepository.AddAsync(friendship);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AcceptRequestAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RejectRequestAsync(int requestId)
        {
            try
            {
                var query = _friendRequestRepository.GetAllQuery();
                var request = await query.FirstOrDefaultAsync(fr => fr.Id == requestId);

                if (request == null || request.Status != FriendRequestStatus.Pending)
                {
                    Console.WriteLine("Request not found or already processed");
                    throw new InvalidOperationException("The request does not exist or has already been processed");
                }

                request.Status = FriendRequestStatus.Rejected;
                request.RespondedAt = DateTime.UtcNow;
                await _friendRequestRepository.UpdateAsync(requestId, request);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RejectRequestAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CanSendRequestAsync(string senderId, string receiverId)
        {
            try
            {
                // Check both conditions in a single query
                var requestQuery = _friendRequestRepository.GetAllQuery();
                var existingPendingRequest = await requestQuery
                    .AnyAsync(fr =>
                        ((fr.SenderId == senderId && fr.ReceiverId == receiverId) ||
                         (fr.SenderId == receiverId && fr.ReceiverId == senderId)) &&
                        fr.Status == FriendRequestStatus.Pending);

                if (existingPendingRequest)
                    return false;

                // Check if they are already friends
                var user1Id = string.Compare(senderId, receiverId) < 0 ? senderId : receiverId;
                var user2Id = string.Compare(senderId, receiverId) < 0 ? receiverId : senderId;

                var friendshipQuery = _friendshipRepository.GetAllQuery();
                var areFriends = await friendshipQuery
                    .AnyAsync(f => f.User1Id == user1Id && f.User2Id == user2Id);

                return !areFriends;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CanSendRequestAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetPendingRequestsCountAsync(string userId)
        {
            try
            {
                var query = _friendRequestRepository.GetAllQuery();

                var count = await query
                    .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                    .CountAsync();

                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPendingRequestsCountAsync: {ex.Message}");
                throw;
            }
        }
    }
}