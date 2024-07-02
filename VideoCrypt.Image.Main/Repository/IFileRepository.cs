using Microsoft.AspNetCore.Mvc;

namespace VideoCrypt.Image.Main.Repository
{
    public interface IFileRepository
    {
        Task UploadFileAsync(IFormFile? file);
        Task<byte[]> GetImageAsync(string fileName);
        Task<List<byte[]>> ListFilesAsync();
    }
}