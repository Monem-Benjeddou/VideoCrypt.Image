using System.Security.Claims;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Repositories
{
    public interface IImageRepository
    {
        Task<bool> DeleteAsync(string fileName, ClaimsPrincipal user);
        Task<string> GetImageAsync(string name, ClaimsPrincipal user);
        Task<ImageResponse> UploadFileAsync(IFormFile file,CancellationToken cancellationToken, ClaimsPrincipal userClaims);
        Task<bool> FileExistsAsync(string key, string userId);
        Task<PaginatedList<string>> ListImagesAsync(int page,int pageSize,string searchQuery ,ClaimsPrincipal user);
        Task<string> ResizeImageAsync(string fileName, int width,
            int height, ImageModificationType type, ClaimsPrincipal user);
        Task<byte[]> DownloadFileAsync(string fileName, ClaimsPrincipal user);
    }
}
