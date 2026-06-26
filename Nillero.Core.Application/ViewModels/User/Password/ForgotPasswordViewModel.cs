using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Application.ViewModels.Login.Password
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        public required string UserName { get; set; } = string.Empty;
    }

}
