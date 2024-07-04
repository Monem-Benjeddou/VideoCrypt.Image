namespace VideoCrypt.Image.Server.Repository
{
    public interface IImageRepository
    {
        Task<string> GetSharedFileUrlAsync(string fileName);
        Task<List<string>> ListImagesAsync(int page, int pageSize);
        Task<bool> DeleteFileFromBucketAsync(string fileName);
        Task<bool> DeleteCachedFileAsync(string fileName);
    }
}