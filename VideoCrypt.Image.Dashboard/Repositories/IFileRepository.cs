using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Dashboard.Models;

namespace VideoCrypt.Image.Dashboard.Repositories
{
    public interface IFileRepository
    {
        Task UploadFileAsync(IFormFile? file);
        Task<byte[]> GetImageAsync(string fileName);
        Task<PaginatedList<string>>ListFilesAsync(int page = 1, int pageSize = 10, string searchQuery="");
        Task DeleteFileAsync(string fileName);
        Task<string> GenerateFileLink(string fileName);

    }
}