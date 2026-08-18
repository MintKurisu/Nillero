using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nillero.Core.Application.Dtos.Social;
using Nillero.Core.Application.Interfaces.Presentation.Mappers;
using Nillero.Core.Application.Interfaces.Social;
using Nillero.Core.Application.Interfaces.Storage;
using Nillero.Core.Application.Interfaces.User;
using Nillero.Core.Application.ViewModels.Social.Comment;
using Nillero.Core.Application.ViewModels.Social.Posts;
using Nillero.Core.Domain.Common.Enum.Social;
using Nillero.Infrastructure.Identity.Entities;

namespace NilleroWebApp.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly IPostReactionService _reactionService;
        private readonly IPostViewModelMapper _postViewModelMapper;
        private readonly IAccountServicesForWebApp _accountService;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public HomeController(
            IPostService postService,
            ICommentService commentService,
            IPostReactionService reactionService,
            IAccountServicesForWebApp accountService,
            UserManager<ApplicationUser> userManager,
            IStorageService storageService,
            IMapper mapper,
            IPostViewModelMapper postViewModelMapper)
            : base(userManager)
        {
            _postService = postService;
            _commentService = commentService;
            _reactionService = reactionService;
            _accountService = accountService;
            _storageService = storageService;
            _mapper = mapper;
            _postViewModelMapper = postViewModelMapper;
        }

        public async Task<IActionResult> Index()
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewData["UserAvatarUrl"] = currentUser.ProfilePicturePath;
            ViewData["UserHandle"] = currentUser.UserName;

            var postsDto = await _postService.GetPostsByUserIdAsync(currentUser.Id);
            var postViewModels = await _postViewModelMapper.MapAsync(postsDto, currentUser.Id);

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
                string mediaPath = await _storageService.UploadAsync(vm.ImageFile, "posts", createdPost.Id.ToString());
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
                if (!string.IsNullOrWhiteSpace(existingPost.MediaPath))
                    await _storageService.DeleteAsync(existingPost.MediaPath);

                existingPost.MediaPath = await _storageService.UploadAsync(vm.ImageFile, "posts", vm.Id.Value.ToString());
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

            if (!string.IsNullOrWhiteSpace(postDto.MediaPath))
                await _storageService.DeleteAsync(postDto.MediaPath);

            await _postService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(SaveCommentViewModel vm, string? returnUrl)
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
        public async Task<IActionResult> EditComment(SaveCommentViewModel vm, string? returnUrl)
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

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id, string? returnUrl)
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
        public async Task<IActionResult> ToggleReaction(int postId, string reactionType, string? returnUrl)
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

        public async Task<IActionResult> Details(int id, int? commentId = null)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Index", "Login");

            var postDto = await _postService.GetById(id);
            if (postDto == null)
                return RedirectToAction("Index");

            var postVms = await _postViewModelMapper.MapAsync(new List<PostDto> { postDto }, currentUser.Id);

            ViewData["UserAvatarUrl"] = currentUser.ProfilePicturePath;
            ViewData["UserHandle"] = currentUser.UserName;
            ViewData["HighlightCommentId"] = commentId; 

            return View("Index", postVms);
        }
    }
}
