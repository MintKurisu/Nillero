using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Interfaces.User;
using Nillero.Core.Application.ViewModels.Social.FriendRequest;
using Nillero.Core.Domain.Common.Enum.Social;
using Nillero.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NilleroWebApp.Controllers
{
    [Authorize]
    public class FriendRequestController : Controller 
    {
        private readonly IFriendRequestService _friendRequestService; 
        private readonly IFriendshipService _friendshipService;
        private readonly IAccountServicesForWebApp _accountService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public FriendRequestController(
            IFriendRequestService friendRequestService,
            IFriendshipService friendshipService,
            IAccountServicesForWebApp accountService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _friendRequestService = friendRequestService;
            _friendshipService = friendshipService;
            _accountService = accountService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var viewModel = new FriendRequestsPageViewModel();

            var pendingRequests = await _friendRequestService.GetPendingReceivedRequestsAsync(currentUser.Id);
            viewModel.PendingRequests = await MapPendingRequestsToViewModels(pendingRequests, currentUser.Id);
            viewModel.PendingCount = viewModel.PendingRequests.Count;

            var sentRequests = await _friendRequestService.GetSentRequestsAsync(currentUser.Id);
            viewModel.SentRequests = await MapSentRequestsToViewModels(sentRequests, currentUser.Id);

            return View(viewModel);
        }

        public async Task<IActionResult> Accept(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var allRequests = await _friendRequestService.GetAll();
            var request = allRequests.FirstOrDefault(r => r.Id == id);

            if (request == null || request.ReceiverId != currentUser.Id)
                return RedirectToAction("Index");

            var senderUser = await _userManager.FindByIdAsync(request.SenderId);
            if (senderUser == null)
                return RedirectToAction("Index");

            ViewBag.RequestId = id;
            ViewBag.SenderUserName = senderUser.UserName;

            return View("AcceptRequest");
        }

        [HttpPost]
        public async Task<IActionResult> AcceptConfirmed(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var allRequests = await _friendRequestService.GetAll();
            var request = allRequests.FirstOrDefault(r => r.Id == id);

            if (request == null || request.ReceiverId != currentUser.Id)
                return RedirectToAction("Index");

            await _friendRequestService.AcceptRequestAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Reject(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var allRequests = await _friendRequestService.GetAll();
            var request = allRequests.FirstOrDefault(r => r.Id == id);

            if (request == null || request.ReceiverId != currentUser.Id)
                return RedirectToAction("Index");

            var senderUser = await _userManager.FindByIdAsync(request.SenderId);
            if (senderUser == null)
                return RedirectToAction("Index");

            ViewBag.RequestId = id;
            ViewBag.SenderUserName = senderUser.UserName;

            return View("RejectRequest");
        }

        [HttpPost]
        public async Task<IActionResult> RejectConfirmed(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var allRequests = await _friendRequestService.GetAll();
            var request = allRequests.FirstOrDefault(r => r.Id == id);

            if (request == null || request.ReceiverId != currentUser.Id)
                return RedirectToAction("Index");

            await _friendRequestService.RejectRequestAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var allRequests = await _friendRequestService.GetAll();
            var request = allRequests.FirstOrDefault(r => r.Id == id);

            if (request == null || request.SenderId != currentUser.Id)
                return RedirectToAction("Index");

            var receiverUser = await _userManager.FindByIdAsync(request.ReceiverId);
            if (receiverUser == null)
                return RedirectToAction("Index");

            ViewBag.RequestId = id;
            ViewBag.ReceiverUserName = receiverUser.UserName;

            return View("DeleteRequest");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var allRequests = await _friendRequestService.GetAll();
            var request = allRequests.FirstOrDefault(r => r.Id == id);

            if (request == null || request.SenderId != currentUser.Id)
                return RedirectToAction("Index");

            await _friendRequestService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SelectUser(string? searchTerm = null)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var allUsers = await _accountService.GetAllUser(true, searchTerm);
            var availableUsers = new List<AvailableUserViewModel>();

            foreach (var user in allUsers)
            {
                if (user.Id == currentUser.Id)
                    continue;

                var areFriends = await _friendshipService.AreFriendsAsync(currentUser.Id, user.Id);
                if (areFriends)
                    continue;

                var canSend = await _friendRequestService.CanSendRequestAsync(currentUser.Id, user.Id);
                if (!canSend)
                    continue;

                var mutualCount = await _friendshipService.GetCommonFriendsCountAsync(currentUser.Id, user.Id);

                var availableUser = new AvailableUserViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = $"{user.FirstName} {user.LastName}",
                    ProfilePicture = user.ProfilePicturePath,
                    MutualFriendsCount = mutualCount
                };

                availableUsers.Add(availableUser);
            }

            ViewBag.SearchTerm = searchTerm ?? "";
            return View(availableUsers);
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(string selectedUserId)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrEmpty(selectedUserId))
            {
                TempData["ErrorMessage"] = "You must select a user..";
                return RedirectToAction("SelectUser");
            }

            var canSend = await _friendRequestService.CanSendRequestAsync(currentUser.Id, selectedUserId);
            if (!canSend)
            {
                TempData["ErrorMessage"] = "A friend request cannot be sent to this user.";
                return RedirectToAction("SelectUser");
            }

            var requestDto = new FriendRequestDto
            {
                SenderId = currentUser.Id,
                ReceiverId = selectedUserId,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _friendRequestService.AddAsync(requestDto);

            TempData["SuccessMessage"] = "Friend request sent successfully.";
            return RedirectToAction("Index");
        }

        #region Private Methods

        private async Task<List<PendingFriendRequestViewModel>> MapPendingRequestsToViewModels(List<FriendRequestDto> requests, string currentUserId)
        {
            var viewModels = new List<PendingFriendRequestViewModel>();

            foreach (var request in requests)
            {
                var senderUser = await _userManager.FindByIdAsync(request.SenderId);
                if (senderUser == null)
                    continue;

                var mutualCount = await _friendshipService.GetCommonFriendsCountAsync(currentUserId, request.SenderId);

                var vm = new PendingFriendRequestViewModel
                {
                    Id = request.Id,
                    SenderId = request.SenderId,
                    SenderUserName = senderUser.UserName ?? "",
                    SenderFullName = $"{senderUser.FirstName} {senderUser.LastName}",
                    SenderProfilePicture = senderUser.ProfilePicturePath,
                    MutualFriendsCount = mutualCount,
                    CreatedAt = request.CreatedAt
                };

                viewModels.Add(vm);
            }

            return viewModels;
        }

        private async Task<List<SentFriendRequestViewModel>> MapSentRequestsToViewModels(List<FriendRequestDto> requests, string currentUserId)
        {
            var viewModels = new List<SentFriendRequestViewModel>();

            foreach (var request in requests)
            {
                var receiverUser = await _userManager.FindByIdAsync(request.ReceiverId);
                if (receiverUser == null)
                    continue;

                var mutualCount = await _friendshipService.GetCommonFriendsCountAsync(currentUserId, request.ReceiverId);

                var vm = new SentFriendRequestViewModel
                {
                    Id = request.Id,
                    ReceiverId = request.ReceiverId,
                    ReceiverUserName = receiverUser.UserName ?? "",
                    ReceiverFullName = $"{receiverUser.FirstName} {receiverUser.LastName}",
                    ReceiverProfilePicture = receiverUser.ProfilePicturePath,
                    MutualFriendsCount = mutualCount,
                    Status = request.Status,
                    CreatedAt = request.CreatedAt
                };

                viewModels.Add(vm);
            }

            return viewModels;
        }

        #endregion
    }
}
