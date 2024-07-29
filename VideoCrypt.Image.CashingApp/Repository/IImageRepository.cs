using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.CashingApp.Repository
{
    public interface IImageRepository
    {
        Task<string> GetSharedFileUrlAsync(string fileName, string userId, int height = 0,int width = 0,ImageModificationType type=ImageModificationType.Crop );
        Task<PaginatedList<string>> ListImagesAsync(string userId, int page, int pageSize, string searchQuery);
        Task<bool> DeleteFileFromBucketAsync(string fileName, string userId);
        Task<bool> DeleteCachedFileAsync(string fileName, string userId);
        Task<bool> PurgeCacheAsync(string userId);
    }
}