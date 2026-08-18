using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Nillero.Core.Application.Dtos.Notifications;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Notifications;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Services.Base;
using Nillero.Core.Domain.Common.Enum;
using Nillero.Core.Domain.Common.Enum.Social;
using Nillero.Core.Domain.Entities.Notifications;
using Nillero.Core.Domain.Entities.Social;
using Nillero.Core.Domain.Interfaces.Notifications;
using Nillero.Core.Domain.Interfaces.Social;

namespace Nillero.Core.Application.Services.Social
{
    public class FriendRequestService : GenericService<FriendRequest, FriendRequestDto>, IFriendRequestService
    {
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IRealTimeNotificationService _rtNotificationService;
        private readonly IMapper _mapper;

        public FriendRequestService(
            IFriendRequestRepository friendRequestRepository,
            IFriendshipRepository friendshipRepository,
            INotificationRepository notificationRepository,
            IRealTimeNotificationService rtNotificationService,
            IMapper mapper) : base(friendRequestRepository, mapper)
        {
            _friendRequestRepository = friendRequestRepository;
            _friendshipRepository = friendshipRepository;
            _notificationRepository = notificationRepository;
            _rtNotificationService = rtNotificationService;
            _mapper = mapper;
        }

        public override async Task<FriendRequestDto?> AddAsync(FriendRequestDto dto)
        {
            try
            {
                var entity = _mapper.Map<FriendRequest>(dto);
                var returned = await _friendRequestRepository.AddAsync(entity);

                if (returned == null)
                    return null;

                // Notify the receiver that they got a friend request
                try
                {
                    if (!string.IsNullOrWhiteSpace(returned.ReceiverId) &&
                        returned.ReceiverId != returned.SenderId)
                    {
                        var notificationDto = new NotificationDto
                        {
                            UserId = returned.ReceiverId,
                            ActorUserId = returned.SenderId,
                            Type = NotificationType.FriendRequest,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        var notification = _mapper.Map<Notification>(notificationDto);
                        await _notificationRepository.AddAsync(notification);

                        var unreadCount = await _notificationRepository.GetUnreadCountAsync(returned.ReceiverId);

                        await _rtNotificationService.SendNotificationAsync(
                            userId: returned.ReceiverId,
                            message: "You have a new friend request.",
                            iconClass: "ph-fill ph-user-plus",
                            unreadCount: unreadCount);
                    }
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"[Notif ERROR] {notifEx.GetType().Name}: {notifEx.Message}");

                    if (notifEx.InnerException != null)
                    {
                        Console.WriteLine($"[Notif INNER] {notifEx.InnerException.GetType().Name}: {notifEx.InnerException.Message}");
                    }

                    Console.WriteLine(notifEx.StackTrace);
                }

                return _mapper.Map<FriendRequestDto>(returned);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddAsync: {ex.Message}");
                throw;
            }
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
                    .Where(fr => fr.SenderId == userId && fr.Status == FriendRequestStatus.Pending)
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

                // Notify the sender that their request was accepted
                try
                {
                    if (!string.IsNullOrWhiteSpace(request.SenderId) &&
                        request.SenderId != request.ReceiverId)
                    {
                        var notificationDto = new NotificationDto
                        {
                            UserId = request.SenderId,
                            ActorUserId = request.ReceiverId,
                            Type = NotificationType.FriendAccepted,
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        var notification = _mapper.Map<Notification>(notificationDto);
                        await _notificationRepository.AddAsync(notification);

                        var unreadCount = await _notificationRepository.GetUnreadCountAsync(request.SenderId);

                        await _rtNotificationService.SendNotificationAsync(
                            userId: request.SenderId,
                            message: "Your friend request was accepted.",
                            iconClass: "ph-fill ph-users",
                            unreadCount: unreadCount);
                    }
                }
                catch (Exception notifEx)
                {
                    Console.WriteLine($"[Notif ERROR] {notifEx.GetType().Name}: {notifEx.Message}");

                    if (notifEx.InnerException != null)
                    {
                        Console.WriteLine($"[Notif INNER] {notifEx.InnerException.GetType().Name}: {notifEx.InnerException.Message}");
                    }

                    Console.WriteLine(notifEx.StackTrace);
                }

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
                var requestQuery = _friendRequestRepository.GetAllQuery();
                var existingPendingRequest = await requestQuery
                    .AnyAsync(fr =>
                        ((fr.SenderId == senderId && fr.ReceiverId == receiverId) ||
                         (fr.SenderId == receiverId && fr.ReceiverId == senderId)) &&
                        fr.Status == FriendRequestStatus.Pending);

                if (existingPendingRequest)
                    return false;

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