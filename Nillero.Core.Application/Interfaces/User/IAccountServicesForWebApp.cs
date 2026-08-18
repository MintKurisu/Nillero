using Nillero.Core.Application.Dtos.User;
using Nillero.Core.Application.Dtos.User.Password;

namespace Nillero.Core.Application.Interfaces.User
{
    public interface IAccountServicesForWebApp
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task<UserResponseDto> ConfirmAccountAsync(string userId, string token);
        Task<UserResponseDto> DeleteAsync(string id);
        Task<EditResponseDto> EditUser(SaveUserDto saveDto, string origin, bool? isCreated = false);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<List<UserDto>> GetAllUser(bool? isActive = true, string? searchTerm = null);
        Task<UserDto?> GetUserByEmail(string email);
        Task<UserDto?> GetUserByUsername(string userName);
        Task<UserDto?> GetUserById(string id);
        Task<RegisterResponseDto> RegisterUser(SaveUserDto saveDto, string origin);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task SignOutAsync();
    }
}