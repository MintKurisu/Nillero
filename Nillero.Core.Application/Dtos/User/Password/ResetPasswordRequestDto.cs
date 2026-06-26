namespace Nillero.Core.Application.Dtos.User.Password
{
    public class ResetPasswordRequestDto
    {
        public required string Id { get; set; }
        public required string Token { get; set; }
        public required string Password { get; set; }

    }
}
