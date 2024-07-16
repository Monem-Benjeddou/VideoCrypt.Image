using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.CashingApp.Repository
{
    public interface IImageRepository
    {
        Task<string> GetSharedFileUrlAsync(string fileName, string userId);
        Task<PaginatedList<string>> ListImagesAsync(string userId, int page, int pageSize);
        Task<bool> DeleteFileFromBucketAsync(string fileName, string userId);
        Task<bool> DeleteCachedFileAsync(string fileName, string userId);
    }
}