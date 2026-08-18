using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Presentation.Mappers;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Interfaces.User;
using Nillero.Core.Application.ViewModels.Social.Comment;
using Nillero.Core.Application.ViewModels.Social.Friendship;
using Nillero.Core.Domain.Common.Enum.Social;
using Nillero.Infrastructure.Identity.Entities;

namespace NilleroWebApp.Controllers
{
    [Authorize]
    public class FriendsController : BaseController
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IPostViewModelMapper _postViewModelMapper;
        private readonly IPostReactionService _reactionService;
        private readonly IAccountServicesForWebApp _accountService;
        private readonly IMapper _mapper;

        public FriendsController(
            IFriendshipService friendshipService,
            IPostService postService,
            ICommentService commentService,
            IPostViewModelMapper postViewModelMapper,
            IPostReactionService reactionService,
            IAccountServicesForWebApp accountService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper) : base(userManager)
        {
            _friendshipService = friendshipService;
            _postService = postService;
            _commentService = commentService;
            _postViewModelMapper = postViewModelMapper;
            _reactionService = reactionService;
            _accountService = accountService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewData["ActiveNav"] = "friends";

            var viewModel = new FriendsPageViewModel();

            var friendships = await _friendshipService.GetFriendsAsync(currentUser.Id);
            viewModel.Friends = await MapFriendshipsToViewModels(friendships, currentUser.Id);

            var friendsPosts = await _postService.GetFriendsPostsAsync(currentUser.Id);
            viewModel.FriendsPosts = await _postViewModelMapper.MapAsync(friendsPosts, currentUser.Id);

            return View(viewModel);
        }

        public async Task<IActionResult> UserPosts(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index");

            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var areFriends = await _friendshipService.AreFriendsAsync(currentUser.Id, userId);
            if (!areFriends)
                return RedirectToAction("Index");

            var friendUser = await _userManager.FindByIdAsync(userId);
            if (friendUser == null)
                return RedirectToAction("Index");

            var posts = await _postService.GetPostsByUserIdAsync(userId);
            var postViewModels = await _postViewModelMapper.MapAsync(posts, currentUser.Id);

            ViewBag.FriendUserId = userId;
            ViewBag.FriendUserName = friendUser.UserName;
            ViewBag.FriendFullName = $"{friendUser.FirstName} {friendUser.LastName}";
            ViewBag.FriendProfilePicture = friendUser.ProfilePicturePath;

            return View(postViewModels);
        }

        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrEmpty(friendId))
                return RedirectToAction("Index");

            var friendUser = await _userManager.FindByIdAsync(friendId);
            if (friendUser == null)
                return RedirectToAction("Index");

            var vm = new FriendViewModel
            {
                UserId = friendUser.Id,
                UserName = friendUser.UserName ?? "",
                FullName = $"{friendUser.FirstName} {friendUser.LastName}",
                ProfilePicture = friendUser.ProfilePicturePath,
                FriendshipCreatedAt = DateTime.UtcNow
            };

            return View("RemoveFriend", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFriendConfirmed(string friendId)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrEmpty(friendId))
                return RedirectToAction("Index");

            await _friendshipService.RemoveFriendshipAsync(currentUser.Id, friendId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(SaveCommentViewModel vm, string? returnUrl = null)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
                return RedirectToAction("Index");

            var commentDto = new CommentDto
            {
                PostId = vm.PostId,
                UserId = currentUser.Id,
                ParentCommentId = vm.ParentCommentId,
                RootCommentId = vm.RootCommentId,
                Content = vm.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _commentService.AddAsync(commentDto);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(SaveCommentViewModel vm, string? returnUrl = null)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!vm.Id.HasValue || string.IsNullOrWhiteSpace(vm.Content))
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index");
            }

            var existingComment = await _commentService.GetById(vm.Id.Value);

            if (existingComment == null || existingComment.UserId != currentUser.Id)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index");
            }

            existingComment.Content = vm.Content;
            existingComment.UpdatedAt = DateTime.UtcNow;

            await _commentService.UpdateAsync(existingComment, vm.Id.Value);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id, string? returnUrl = null)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var commentDto = await _commentService.GetById(id);
            if (commentDto == null || commentDto.UserId != currentUser.Id)
                return RedirectToAction("Index");

            await _commentService.DeleteAsync(id);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReaction(int postId, string reactionType, string? returnUrl = null)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ReactionType newReaction = reactionType.ToLower() == "like"
                ? ReactionType.Like
                : ReactionType.Dislike;

            var currentReaction = await _reactionService.GetUserReactionAsync(currentUser.Id, postId);

            if (currentReaction == newReaction)
            {
                // Clicking the same reaction removes it
                await _reactionService.RemoveReactionAsync(currentUser.Id, postId);
            }
            else
            {
                // Creates a new reaction or switches Like <-> Dislike
                await _reactionService.ToggleReactionAsync(currentUser.Id, postId, newReaction);
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index");
        }

        private async Task<List<FriendViewModel>> MapFriendshipsToViewModels(
            List<FriendshipDto> friendships,
            string currentUserId)
        {
            var result = new List<FriendViewModel>();

            foreach (var friendship in friendships)
            {
                var friendId = friendship.User1Id == currentUserId
                    ? friendship.User2Id
                    : friendship.User1Id;

                var user = await _userManager.FindByIdAsync(friendId);

                if (user == null)
                    continue;

                result.Add(new FriendViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? "",
                    FullName = $"{user.FirstName} {user.LastName}",
                    ProfilePicture = user.ProfilePicturePath,
                    FriendshipCreatedAt = friendship.CreatedAt
                });
            }

            return result;
        }
    }
}
