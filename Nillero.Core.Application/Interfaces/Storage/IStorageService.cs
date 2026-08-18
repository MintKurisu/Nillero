using Microsoft.AspNetCore.Http;

namespace Nillero.Core.Application.Interfaces.Storage
{
    public interface IStorageService
    {
        Task<string> UploadAsync(IFormFile file, string folder, string fileName);
        Task DeleteAsync(string fileUrl);
    }
}
