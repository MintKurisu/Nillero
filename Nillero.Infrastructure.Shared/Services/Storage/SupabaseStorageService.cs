using Microsoft.AspNetCore.Http;
using Nillero.Core.Application.Interfaces.Storage;
using Supabase;

namespace Nillero.Infrastructure.Shared.Services.Storage
{
    public class SupabaseStorageService : IStorageService
    {
        private readonly Client _supabaseClient;
        private const string BucketName = "nillero-media";

        public SupabaseStorageService(Client supabaseClient)
        {
            _supabaseClient = supabaseClient;
        }

        public async Task<string> UploadAsync(IFormFile file, string folder, string fileName)
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length);

            /*var filePath = $"{folder}/{fileName}{Path.GetExtension(file.FileName)}";*/

            var filePath = $"{folder}/{fileName}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Path.GetExtension(file.FileName)}";

            await _supabaseClient.Storage
                .From(BucketName)
                .Upload(buffer, filePath, new Supabase.Storage.FileOptions
                {
                    Upsert = true,
                    ContentType = file.ContentType
                });

            var publicUrl = _supabaseClient.Storage
                .From(BucketName)
                .GetPublicUrl(filePath);

            // Cache buster — Forces the browser and CDN to fetch the newly updated image instantly
            /*return $"{publicUrl}?v={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";*/
            return publicUrl;
        }

        public async Task DeleteAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;

            var uri = new Uri(fileUrl);

            // Extract the relative path by removing the Supabase public URL prefix
            var path = uri.AbsolutePath
                .Replace($"/storage/v1/object/public/{BucketName}/", "");

            // uri.AbsolutePath automatically discards query strings like '?v=...',
            // but this safe-split ensures no decoded artifacts remain
            if (path.Contains("?"))
            {
                path = path.Split('?')[0];
            }

            await _supabaseClient.Storage
                .From(BucketName)
                .Remove(new List<string> { path });
        }
    }
}
