using Nillero.Core.Application.Dtos.Email;
using Nillero.Core.Application.Dtos.User;
using Nillero.Core.Application.Dtos.User.Password;
using Nillero.Core.Application.Interfaces.Email;
using Nillero.Core.Application.Interfaces.User;
using Nillero.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Nillero.Infrastructure.Identity.Services
{
    public class AccountServicesForWebApp : IAccountServicesForWebApp
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountServicesForWebApp(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }
        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {

            LoginResponseDto response = new()
            {
                Id = "",
                UserName = "",
                FirstName = "",
                LastName = "",
                Email = "",
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No account found with username {loginDto.UserName}.");
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Errors.Add($"The email for this account ({loginDto.UserName}) is not confirmed or the account is inactive. Check your email.");
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, loginDto.Password, isPersistent: false, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                response.HasError = true;

                if (result.IsLockedOut)
                {
                    response.Errors.Add($"Your account {loginDto.UserName} has been temporarily locked due to multiple failed attempts. Please try signing in again in 10 minutes.");
                }
                else
                {
                    response.Errors.Add($"Sign-in credentials for user {user.UserName} are invalid.");

                }
                return response;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            response.Id = user.Id;
            response.UserName = user.UserName;
            response.FirstName = user.FirstName;
            response.LastName = user.LastName;
            response.Email = user.Email ?? "";
            response.IsVerified = user.EmailConfirmed;
            response.Roles = rolesList.ToList();

            return response;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string origin) 
        {
            RegisterResponseDto response = new()
            {
                Id = "",
                UserName = "",
                FirstName = "",
                LastName = "",
                Email = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(saveDto.UserName);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"This username is already taken: {saveDto.UserName}.");
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"This email is already in use: {saveDto.Email}.");
                return response;
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                UserName = saveDto.UserName,
                FirstName = saveDto.FirstName,
                LastName = saveDto.LastName,
                Email = saveDto.Email,
                EmailConfirmed = false,
                Phone = saveDto.Phone,
                ProfilePicturePath = saveDto.ProfilePicturePath,
                IsActive = saveDto.IsActive,

            };

            var result = await _userManager.CreateAsync(newUser, saveDto.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            if (!await _userManager.IsInRoleAsync(newUser, saveDto.Role)) // por el momento solo tenemos el rol de usuario, pero se deja abierto a extension 
                await _userManager.AddToRoleAsync(newUser, saveDto.Role);                                        

            string verificationUri = await GetVerificationEmailUri(newUser, origin);

            await _emailService.SendAsync(new EmailRequestDto()
            {
                ToEmail = saveDto.Email,
                Subject = "Confirm your LinkUpApp account",
                HtmlBody = $"Hello {newUser.FirstName}, please confirm your account by clicking the following link: {verificationUri}"
            });

            var rolesList = await _userManager.GetRolesAsync(newUser);

            response.Id = newUser.Id;
            response.UserName = newUser.UserName;
            response.FirstName = newUser.FirstName;
            response.LastName = newUser.LastName;
            response.Email = newUser.Email ?? "";
            response.IsVerified = newUser.EmailConfirmed;
            response.Roles = rolesList.ToList();

            return response;

        }
        public async Task<EditResponseDto> EditUser(SaveUserDto saveDto, string origin, bool? isCreated = false) 
        {
            bool isNotCreated = !isCreated ?? false;

            EditResponseDto response = new()
            {
                Id = "",
                UserName = "",
                FirstName = "",
                LastName = "",
                Email = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(saveDto.UserName);
            if (userWithSameUserName != null && userWithSameUserName.Id != saveDto.Id)
            {
                response.HasError = true;
                response.Errors.Add($"This username is already taken: {saveDto.UserName}.");
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null && userWithSameEmail.Id != saveDto.Id)
            {
                response.HasError = true;
                response.Errors.Add($"This email is already in use: {saveDto.Email}.");
                return response;
            }


            var user = await _userManager.FindByIdAsync(saveDto.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No account is registered for this user.");
                return response;
            }

            user.UserName = saveDto.UserName;
            user.FirstName = saveDto.FirstName;
            user.LastName = saveDto.LastName;
            user.EmailConfirmed = user.EmailConfirmed && user.Email == saveDto.Email; // si cambia el email, se debe reconfirmar
            user.Email = saveDto.Email;
            user.Phone = saveDto.Phone;
            user.ProfilePicturePath = string.IsNullOrWhiteSpace(saveDto.ProfilePicturePath) ? user.ProfilePicturePath : saveDto.ProfilePicturePath;
            user.IsActive = saveDto.IsActive;

            // Actualizar contrasenia si se proporciona una nueva

            if (!string.IsNullOrWhiteSpace(saveDto.Password) && isNotCreated)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultChange = await _userManager.ResetPasswordAsync(user, token, saveDto.Password);

                if (resultChange != null && !resultChange.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(resultChange.Errors.Select(s => s.Description).ToList());
                    return response;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, rolesList.ToList());

            await _userManager.AddToRoleAsync(user, saveDto.Role);

            if (!user.EmailConfirmed && isNotCreated) 
            {
                string verificationUri = await GetVerificationEmailUri(user, origin);
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    ToEmail = saveDto.Email,
                    Subject = "Confirm your LinkUpApp account",
                    HtmlBody = $"Hello {user.FirstName}, please confirm your account by clicking the following link: <a href='{verificationUri}'>Confirm Account</a>"
                });
            }

            var updatedRolesList = await _userManager.GetRolesAsync(user);

            response.Id = user.Id;
            response.UserName = user.UserName;
            response.FirstName = user.FirstName;
            response.LastName = user.LastName;
            response.Email = user.Email ?? "";
            response.IsVerified = user.EmailConfirmed;
            response.Roles = updatedRolesList.ToList();

            return response;

        }
        public async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            UserResponseDto response = new()
            {
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No account is registered for this user: {request.UserName}");
                return response;
            }

            var resetUri = await GetResetPasswordUri(user, request.Origin);

            user.IsActive = false;
            user.EmailConfirmed = false;
            await _userManager.UpdateAsync(user);

            await _emailService.SendAsync(new EmailRequestDto()
            {
                ToEmail = user.Email,
                Subject = "Reset password",
                HtmlBody = $"Hello {user.FirstName}, to reset your password please click the following link: {resetUri}"
            });


            return response;
        }
        public async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            UserResponseDto response = new()
            {
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No account is registered for this user.");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            user.IsActive = true;
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return response;
        }
        public async Task<UserResponseDto> DeleteAsync(string id)
        {
            UserResponseDto response = new()
            {
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No account is registered for this user.");
                return response;
            }

            await _userManager.DeleteAsync(user);
            return response;
        }
        public async Task<UserDto?> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return null;

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                Phone = user.Phone,
                ProfilePicturePath = user.ProfilePicturePath,
                Role = rolesList.FirstOrDefault() ?? "",
                IsVerified = user.EmailConfirmed,
                IsActive = user.IsActive,
            };

            return userDto;

        }
        public async Task<UserDto?> GetUserByUsername(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
                return null;

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                Phone = user.Phone,
                ProfilePicturePath = user.ProfilePicturePath,
                Role = rolesList.FirstOrDefault() ?? "",
                IsVerified = user.EmailConfirmed,
                IsActive = user.IsActive,
            };

            return userDto;

        }
        public async Task<UserDto?> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return null;

            var rolesList = await _userManager.GetRolesAsync(user);

            var userDto = new UserDto()
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                Phone = user.Phone,
                ProfilePicturePath = user.ProfilePicturePath,
                Role = rolesList.FirstOrDefault() ?? "",
                IsVerified = user.EmailConfirmed,
                IsActive = user.IsActive,
            };

            return userDto;

        }

        public async Task<List<UserDto>> GetAllUser(bool? isActive = true, string? searchTerm = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (isActive == true)
                query = query.Where(u => u.IsActive && u.EmailConfirmed);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(u =>
                    (u.UserName != null && u.UserName.Contains(searchTerm)) ||
                    (u.FirstName != null && u.FirstName.Contains(searchTerm)) ||
                    (u.LastName != null && u.LastName.Contains(searchTerm)));

            var users = await query.ToListAsync();
            var listUsersDto = new List<UserDto>();

            foreach (var item in users)
            {
                var roleList = await _userManager.GetRolesAsync(item);
                listUsersDto.Add(new UserDto
                {
                    Id = item.Id,
                    UserName = item.UserName ?? "",
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    Email = item.Email ?? "",
                    Phone = item.Phone,
                    ProfilePicturePath = item.ProfilePicturePath,
                    Role = roleList.FirstOrDefault() ?? "",
                    IsVerified = item.EmailConfirmed,
                    IsActive = item.IsActive,
                });
            }

            return listUsersDto;
        }

        public async Task<UserResponseDto> ConfirmAccountAsync(string userId, string token)
        {
            UserResponseDto response = new()
            {
                HasError = false,
                Errors = []
            };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("This user is not registered.");
                return response;
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.Add($"An error occurred while confirming the email {user.Email}.");
                return response;
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);
            return response;
        }

        #region Private Methods
        private async Task<string> GetVerificationEmailUri(ApplicationUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var route = "Login/ConfirmEmail";
            var completeUrl = new Uri(string.Concat(origin, "/", route)); // origin = https://localhost:5628 

            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri.ToString(), "token", token);

            return verificationUri;
        }

        private async Task<string> GetResetPasswordUri(ApplicationUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var route = "Login/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route)); // origin = https://localhost:5628 
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);

            resetUri = QueryHelpers.AddQueryString(resetUri, "token", token);
            return resetUri;
        }

        #endregion
    }
}   
