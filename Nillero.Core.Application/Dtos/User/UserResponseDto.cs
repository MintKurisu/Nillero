namespace Nillero.Core.Application.Dtos.User
{
    public class UserResponseDto
    {
        public bool HasError { get; set; }
        public required List<string>? Errors { get; set; }

    }
}
