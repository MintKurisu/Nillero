using AutoMapper;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Interfaces.User;
using Nillero.Core.Application.ViewModels.Social.Comment;
using Nillero.Core.Application.ViewModels.Social.Posts;
using Nillero.Core.Domain.Common.Enum.Social;
using Nillero.Infrastructure.Identity.Entities;
using NilleroWebApp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NilleroWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller 
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IPostReactionService _reactionService;
        private readonly IAccountServicesForWebApp _accountService; 
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public HomeController(
            IPostService postService,
            ICommentService commentService,
            IPostReactionService reactionService,
            IAccountServicesForWebApp accountService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
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

            var postsDto = await _postService.GetPostsByUserIdAsync(currentUser.Id);
            var postViewModels = await MapPostsToViewModels(postsDto, currentUser.Id);

            return View(postViewModels);
        }

        public async Task<IActionResult> Create()
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View("SavePost", new SavePostViewModel
            {
                Content = "",
                MediaType = "Image"
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SavePostViewModel vm)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (vm.MediaType == "Image" && vm.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "You must upload an image.");
            }

            if (vm.MediaType == "Video" && string.IsNullOrWhiteSpace(vm.YouTubeUrl))
            {
                ModelState.AddModelError("YouTubeUrl", "You must provide a YouTube link.");
            }

            if (!ModelState.IsValid)
                return View("SavePost", vm);

            var postDto = new PostDto
            {
                UserId = currentUser.Id,
                Content = vm.Content,
                Type = vm.MediaType == "Image" ? PostType.Image : PostType.Video,
                MediaPath = null,
                YouTubeUrl = vm.MediaType == "Video" ? vm.YouTubeUrl : null,
                CreatedAt = DateTime.UtcNow
            };

            var createdPost = await _postService.AddAsync(postDto);

            if (createdPost != null && vm.MediaType == "Image" && vm.ImageFile != null)
            {
                string mediaPath = FileManager.Upload(vm.ImageFile, createdPost.Id.ToString(), "Posts");
                createdPost.MediaPath = mediaPath;
                await _postService.UpdateAsync(createdPost, createdPost.Id);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var postDto = await _postService.GetById(id);
            if (postDto == null)
                return RedirectToAction("Index");

            if (postDto.UserId != currentUser.Id)
                return RedirectToAction("Index");

            var vm = _mapper.Map<SavePostViewModel>(postDto);
            ViewBag.CurrentMediaPath = postDto.MediaPath;

            return View("SavePost", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SavePostViewModel vm)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!vm.Id.HasValue)
                return RedirectToAction("Index");

            var existingPost = await _postService.GetById(vm.Id.Value);
            if (existingPost == null || existingPost.UserId != currentUser.Id)
                return RedirectToAction("Index");

            if (vm.MediaType == "Video" && string.IsNullOrWhiteSpace(vm.YouTubeUrl))
            {
                ModelState.AddModelError("YouTubeUrl", "You must provide a YouTube link.");
            }

            if (!ModelState.IsValid)
                return View("SavePost", vm);

            existingPost.Content = vm.Content;
            existingPost.UpdatedAt = DateTime.UtcNow;

            if (vm.MediaType == "Image" && vm.ImageFile != null)
            {
                existingPost.MediaPath = FileManager.Upload(vm.ImageFile, vm.Id.Value.ToString(), "Posts", true, existingPost.MediaPath ?? "");
                existingPost.Type = PostType.Image;
                existingPost.YouTubeUrl = null;
            }
            else if (vm.MediaType == "Video")
            {
                existingPost.YouTubeUrl = vm.YouTubeUrl;
                existingPost.Type = PostType.Video;
                existingPost.MediaPath = null;
            }

            await _postService.UpdateAsync(existingPost, vm.Id.Value);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var postDto = await _postService.GetById(id);
            if (postDto == null)
                return RedirectToAction("Index");

            if (postDto.UserId != currentUser.Id)
                return RedirectToAction("Index");

            var vm = _mapper.Map<PostViewModel>(postDto);
            vm.UserName = currentUser.UserName ?? "";
            vm.UserFullName = $"{currentUser.FirstName} {currentUser.LastName}";
            vm.UserProfilePicture = currentUser.ProfilePicturePath;
            vm.IsOwner = true;

            return View("DeletePost", vm);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var postDto = await _postService.GetById(id);
            if (postDto == null)
                return RedirectToAction("Index");

            if (postDto.UserId != currentUser.Id)
                return RedirectToAction("Index");

            await _postService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(SaveCommentViewModel vm)
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(SaveCommentViewModel vm)
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleReaction(int postId, string reactionType)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ReactionType newReaction = reactionType.ToLower() == "like"
                ? ReactionType.Like
                : ReactionType.Dislike;

            // Get the current user's reaction
            var currentReaction = await _reactionService.GetUserReactionAsync(currentUser.Id, postId);

            // Only call the service if:
            // 1. There is no previous reaction (currentReaction == null), OR
            // 2. The reaction is different from the current one
            if (currentReaction == null || currentReaction != newReaction)
            {
                await _reactionService.ToggleReactionAsync(currentUser.Id, postId, newReaction);
            }
            // If currentReaction == newReaction, do nothing (cannot remove)

            return RedirectToAction("Index");
        }

        #region Private Methods

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
