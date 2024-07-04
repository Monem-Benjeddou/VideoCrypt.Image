namespace VideoCrypt.Image.Server.Repository
{
    public interface IImageRepository
    {
        Task<string> GetSharedFileUrlAsync(string fileName);
        Task<List<string>> ListImagesAsync();
        Task<bool> DeleteFileFromBucketAsync(string fileName);
        Task<bool> DeleteCachedFileAsync(string fileName);
    }
}