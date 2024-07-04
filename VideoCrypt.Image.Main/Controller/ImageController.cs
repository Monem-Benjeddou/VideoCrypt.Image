using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Controllers;

public class ImageController : Controller
{
    private readonly IFileRepository _fileRepository;

    public ImageController(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(string imageUrl)
    {
        try
        {
            var fileName = imageUrl.Split('/').Last();
            await _fileRepository.DeleteFileAsync(fileName);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }
}