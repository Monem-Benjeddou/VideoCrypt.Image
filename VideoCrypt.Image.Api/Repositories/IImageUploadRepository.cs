namespace VideoCrypt.Image.Api.Repositories
{
    public interface IImageUploadRepository
    {
        Task<bool> UploadFileAsync(IFormFile file, string userId);
        Task<bool> FileExistsAsync(string key, string userId);
    }
}
