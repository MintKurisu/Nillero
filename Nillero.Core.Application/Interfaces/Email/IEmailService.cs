using Nillero.Core.Application.Dtos.Email;

namespace Nillero.Core.Application.Interfaces.Email
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
    }
}
