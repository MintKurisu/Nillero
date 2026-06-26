using System.ComponentModel.DataAnnotations;

namespace Nillero.Core.Application.ViewModels.Login
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; } = string.Empty;
    }

}
