namespace VideoCrypt.Image.Api.Utilities;

public interface IImageUploadRepository
{
    Task<bool> UploadFileAsync(IFormFile file);
    Task<bool> FileExistsAsync(string key);
}