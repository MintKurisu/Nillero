using AutoMapper;
using Nillero.Core.Application.Dtos.User;
using Nillero.Core.Application.Interfaces.User;
using Nillero.Core.Application.ViewModels.Social.User;
using Nillero.Core.Domain.Common.Enum;
using Nillero.Infrastructure.Identity.Entities;
using NilleroWebApp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NilleroWebApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IAccountServicesForWebApp _accountService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public ProfileController(
            IAccountServicesForWebApp accountService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
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

            var vm = new EditProfileViewModel
            {
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Phone = currentUser.Phone,
                CurrentProfilePicturePath = currentUser.ProfilePicturePath,
                Password = string.Empty,
                ConfirmPassword = string.Empty
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(EditProfileViewModel vm)
        {
            ApplicationUser? currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                if (vm.Password != vm.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                }
            }

            if (!ModelState.IsValid)
                return View(vm);

            string? profilePicturePath = vm.CurrentProfilePicturePath;
            if (vm.ProfilePicture != null)
            {
                profilePicturePath = FileManager.Upload(vm.ProfilePicture, currentUser.Id, "Users", true, vm.CurrentProfilePicturePath ?? "");
            }

            var rolesList = await _userManager.GetRolesAsync(currentUser);
            string currentRole = rolesList.FirstOrDefault() ?? Roles.User.ToString();

            SaveUserDto dto = new SaveUserDto
            {
                Id = currentUser.Id,
                UserName = currentUser.UserName ?? "",
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = currentUser.Email ?? "",
                Phone = vm.Phone,
                Password = vm.Password ?? "", 
                ProfilePicturePath = profilePicturePath,
                Role = currentRole,
                IsActive = currentUser.IsActive
            };

            string origin = $"{Request.Scheme}://{Request.Host}";

            var response = await _accountService.EditUser(dto, origin, false);

            if (response.HasError)
            {
                ViewBag.hasError = true;
                ViewBag.Errors = response.Errors;
                return View(vm);
            }

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("Index");
        }
    }

}
