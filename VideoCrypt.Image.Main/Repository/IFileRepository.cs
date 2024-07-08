using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Main.Models;

namespace VideoCrypt.Image.Main.Repository
{
    public interface IFileRepository
    {
        Task UploadFileAsync(IFormFile? file);
        Task<byte[]> GetImageAsync(string fileName);
        Task<PaginatedList<string>> ListFilesAsync(int page = 1, int pageSize = 10);
        Task DeleteFileAsync(string fileName);
        Task<string> GenerateFileLink(string fileName);

    }
}