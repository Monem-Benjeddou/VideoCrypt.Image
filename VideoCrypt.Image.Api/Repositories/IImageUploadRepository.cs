namespace VideoCrypt.Image.Api.Utilities;

public interface IImageUploadRepository
{
    Task<bool> UploadFileAsync(string fileName, MemoryStream memoryStream, string contentType);
    Task<bool> FileExistsAsync(string key);
}