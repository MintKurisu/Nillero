namespace Nillero.Core.Application.Dtos.User.Password
{
    public class ForgotPasswordRequestDto
    {
        public required string UserName {  get; set; }
        public required string Origin { get; set; }
    }
}
