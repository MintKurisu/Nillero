using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Interfaces.User;
using Nillero.Core.Application.ViewModels.Social.Comment;
using Nillero.Core.Application.ViewModels.Social.Friendship;
using Nillero.Core.Application.ViewModels.Social.Posts;
using Nillero.Core.Domain.Common.Enum.Social;
using Nillero.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NilleroWebApp.Controllers
{
    [Authorize]
    public class FriendsController : Controller 
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IPostService _postService;
        private readonly ICommentService _commentService; 
        private readonly IPostReactionService _reactionService;
        private readonly IAccountServicesForWebApp _accountService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public FriendsController(
            IFriendshipService friendshipService,
            IPostService postService,
            ICommentService commentService,
            IPostReactionService reactionService,
            IAccountServicesForWebApp accountService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _friendshipService = friendshipService;
            _postService = postService;
            _commentService = commentService;
            _reactionService = reactionService;
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

            var viewModel = new FriendsPageViewModel();

            var friendships = await _friendshipService.GetFriendsAsync(currentUser.Id);
            viewModel.Friends = await MapFriendshipsToViewModels(friendships, currentUser.Id);

            var friendsPosts = await _postService.GetFriendsPostsAsync(currentUser.Id);
            viewModel.FriendsPosts = await MapPostsToViewModels(friendsPosts, currentUser.Id);

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
            var postViewModels = await MapPostsToViewModels(posts, currentUser.Id);

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
        public async Task<IActionResult> AddComment(SaveCommentViewModel vm, string? returnUserId = null)
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

            if (!string.IsNullOrEmpty(returnUserId))
            {
                return RedirectToAction("UserPosts", new { userId = returnUserId });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(SaveCommentViewModel vm, string? returnUserId = null)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid || !vm.Id.HasValue)
                return RedirectToAction("Index");

            var existingComment = await _commentService.GetById(vm.Id.Value);
            if (existingComment == null || existingComment.UserId != currentUser.Id)
                return RedirectToAction("Index");

            existingComment.Content = vm.Content;
            existingComment.UpdatedAt = DateTime.UtcNow;

            await _commentService.UpdateAsync(existingComment, vm.Id.Value);

            if (!string.IsNullOrEmpty(returnUserId))
            {
                return RedirectToAction("UserPosts", new { userId = returnUserId });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id, string? returnUserId = null)
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

            if (!string.IsNullOrEmpty(returnUserId))
            {
                return RedirectToAction("UserPosts", new { userId = returnUserId });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReaction(int postId, string reactionType, string? returnUserId = null)
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

            if (currentReaction == null || currentReaction != newReaction)
            {
                await _reactionService.ToggleReactionAsync(currentUser.Id, postId, newReaction);
            }

            if (!string.IsNullOrEmpty(returnUserId))
            {
                return RedirectToAction("UserPosts", new { userId = returnUserId });
            }

            return RedirectToAction("Index");
        }

        #region Private Methods

        private async Task<List<FriendViewModel>> MapFriendshipsToViewModels(List<FriendshipDto> friendships, string currentUserId)
        {
            var friendViewModels = new List<FriendViewModel>();

            foreach (var friendship in friendships)
            {
                string friendId = friendship.User1Id == currentUserId ? friendship.User2Id : friendship.User1Id;
                var friendUser = await _userManager.FindByIdAsync(friendId);

                if (friendUser != null)
                {
                    var friendVm = new FriendViewModel
                    {
                        UserId = friendUser.Id,
                        UserName = friendUser.UserName ?? "",
                        FullName = $"{friendUser.FirstName} {friendUser.LastName}",
                        ProfilePicture = friendUser.ProfilePicturePath,
                        FriendshipCreatedAt = friendship.CreatedAt
                    };

                    friendViewModels.Add(friendVm);
                }
            }

            return friendViewModels;
        }

        private async Task<List<PostViewModel>> MapPostsToViewModels(List<PostDto> postsDto, string currentUserId)
        {
            var postViewModels = new List<PostViewModel>();

            foreach (var postDto in postsDto)
            {
                var postOwner = await _userManager.FindByIdAsync(postDto.UserId);
                var comments = await _commentService.GetCommentsByPostIdAsync(postDto.Id);
                var (likes, dislikes) = await _reactionService.GetReactionCountsAsync(postDto.Id);
                var userReaction = await _reactionService.GetUserReactionAsync(currentUserId, postDto.Id);

                var postVm = _mapper.Map<PostViewModel>(postDto);
                postVm.UserName = postOwner?.UserName ?? "";
                postVm.UserFullName = $"{postOwner?.FirstName} {postOwner?.LastName}";
                postVm.UserProfilePicture = postOwner?.ProfilePicturePath;
                postVm.IsOwner = postDto.UserId == currentUserId;
                postVm.LikeCount = likes;
                postVm.DislikeCount = dislikes;
                postVm.UserReaction = userReaction;
                postVm.Comments = await MapCommentsToViewModels(comments, currentUserId);

                postViewModels.Add(postVm);
            }

            return postViewModels;
        }

        private async Task<List<CommentViewModel>> MapCommentsToViewModels(List<CommentDto> commentsDto, string currentUserId)
        {
            var commentViewModels = new List<CommentViewModel>();

            foreach (var c in commentsDto)
            {
                var commentOwner = await _userManager.FindByIdAsync(c.UserId);

                var commentVm = _mapper.Map<CommentViewModel>(c);
                commentVm.UserName = commentOwner?.UserName ?? "";
                commentVm.UserFullName = $"{commentOwner?.FirstName} {commentOwner?.LastName}";
                commentVm.UserProfilePicture = commentOwner?.ProfilePicturePath;
                commentVm.IsOwner = c.UserId == currentUserId;

                commentVm.Replies = await MapCommentsToViewModels(c.Replies, currentUserId);

                commentViewModels.Add(commentVm);
            }

            return commentViewModels;
        }

        #endregion
    }
}
