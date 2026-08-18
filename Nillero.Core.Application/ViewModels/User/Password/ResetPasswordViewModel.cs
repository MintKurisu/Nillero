using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Application.ViewModels.Login.Password
{
    public class ResetPasswordViewModel
    {
        [DataType(DataType.Text)]
        public required string Token { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        public required string UserId { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public required string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public required string ConfirmPassword { get; set; } = string.Empty;
    }
}
