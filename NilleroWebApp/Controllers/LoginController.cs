using Nillero.Core.Application.Dtos.User;
using Nillero.Core.Application.Dtos.User.Password;
using Nillero.Core.Application.Interfaces.User;
using Nillero.Core.Application.ViewModels.Login;
using Nillero.Core.Application.ViewModels.Login.Password;
using Nillero.Core.Domain.Common.Enum;
using Nillero.Infrastructure.Identity.Entities;
using NilleroWebApp.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NilleroWebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAccountServicesForWebApp _accountServiceForWebApp;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginController(
            IAccountServicesForWebApp accountService,
            UserManager<ApplicationUser> userManager)
        {
            _accountServiceForWebApp = accountService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? returnUrl = null)
        {
            ApplicationUser? userSession = await _userManager.GetUserAsync(User);

            if (userSession != null)
            {
                var user = await _accountServiceForWebApp.GetUserByUsername(userSession.UserName ?? "");
                if (user != null && user.Role == Roles.User.ToString())
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            if (!string.IsNullOrEmpty(returnUrl) &&
                returnUrl != "/" &&
                !returnUrl.Contains("/Login", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.UnauthorizedMessage = "You must log in to access this section.";
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(new LoginViewModel
            {
                UserName = string.Empty,
                Password = string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(vm);
            }

            var loginDto = new LoginDto
            {
                UserName = vm.UserName,
                Password = vm.Password
            };

            var result = await _accountServiceForWebApp.AuthenticateAsync(loginDto);

            if (result.HasError)
            {
                ViewBag.ReturnUrl = returnUrl;

                if (result.Errors.Any())
                {
                    ViewBag.ErrorMessage = result.Errors.First();
                }

                return View(vm);
            }

            if (!string.IsNullOrEmpty(returnUrl) &&
                returnUrl != "/" &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _accountServiceForWebApp.SingOutAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Register()
        {
            return View(new RegisterViewModel
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                Phone = string.Empty,
                Email = string.Empty,
                UserName = string.Empty,
                Password = string.Empty,
                ConfirmPassword = string.Empty
            });
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            string? profilePicturePath = null;
            if (vm.ProfilePicture != null)
            {
                profilePicturePath = FileManager.Upload(vm.ProfilePicture, vm.UserName, "Users");
            }

            SaveUserDto dto = new SaveUserDto
            {
                Id = null, // for new registrations
                UserName = vm.UserName,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                Phone = vm.Phone,
                Password = vm.Password,
                ProfilePicturePath = profilePicturePath,
                Role = Roles.User.ToString(), // Always "User"
                IsActive = false // Inactive until email confirmation
            };

            string origin = $"{Request.Scheme}://{Request.Host}";

            RegisterResponseDto? response = await _accountServiceForWebApp.RegisterUser(dto, origin);

            if (response.HasError)
            {
                ViewBag.hasError = true;
                ViewBag.Errors = response.Errors;
                return View(vm);
            }

            if (response != null && !string.IsNullOrWhiteSpace(response.Id))
            {
                dto.Id = response.Id;
                dto.ProfilePicturePath = FileManager.Upload(vm.ProfilePicture, dto.Id, "Users");
                await _accountServiceForWebApp.EditUser(dto, origin, true); 
            }

            TempData["SuccessMessage"] = "User registered successfully. Please check your email to activate your account.";
            return RedirectToAction("Index");
        }

        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel
            {
                UserName = string.Empty
            });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            string origin = $"{Request.Scheme}://{Request.Host}";

            ForgotPasswordRequestDto dto = new()
            {
                UserName = vm.UserName,
                Origin = origin
            };

            UserResponseDto? returnUser = await _accountServiceForWebApp.ForgotPasswordAsync(dto);

            if (returnUser.HasError)
            {
                ViewBag.hasError = true;
                ViewBag.Errors = returnUser.Errors;
                return View(vm);
            }

            TempData["SuccessMessage"] = "An email has been sent with instructions to reset your password.";
            return RedirectToAction("Index");
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid reset link.";
                return RedirectToAction("Index");
            }

            var vm = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token,
                Password = string.Empty,
                ConfirmPassword = string.Empty
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            ResetPasswordRequestDto dto = (new ResetPasswordRequestDto
            {
                Id = vm.UserId,
                Token = vm.Token,
                Password = vm.Password
            });

            UserResponseDto? returnUser = await _accountServiceForWebApp.ResetPasswordAsync(dto);

            if (returnUser.HasError)
            {
                ViewBag.hasError = true;
                ViewBag.Errors = returnUser.Errors;
                return View(vm);
            }

            TempData["SuccessMessage"] = "Password reset successfully. You can now log in.";
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid activation link.";
                return RedirectToAction("Index");
            }

            var response = await _accountServiceForWebApp.ConfirmAccountAsync(userId, token);

            if (response.HasError)
                TempData["ErrorMessage"] = response.Errors.FirstOrDefault();
            else
                TempData["SuccessMessage"] = "Account confirmed. You can now log in!";

            return RedirectToAction("Index");
        }

    }
}
