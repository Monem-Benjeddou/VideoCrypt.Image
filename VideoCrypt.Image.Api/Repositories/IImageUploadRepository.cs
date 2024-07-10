namespace VideoCrypt.Image.Api.Repositories;

public interface IImageUploadRepository
{
    Task<bool> UploadFileAsync(IFormFile file);
    Task<bool> FileExistsAsync(string key);
}