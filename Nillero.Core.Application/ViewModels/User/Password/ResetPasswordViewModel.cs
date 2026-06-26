using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Application.ViewModels.Login.Password
{
    public class ResetPasswordViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        public required string Token { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Text)]
        public required string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "New Password")]
        public required string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must confirm the password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public required string ConfirmPassword { get; set; } = string.Empty;
    }
}
