using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UploadFileController(IFileRepository fileRepository) : Controller
{
    private readonly string[] _permittedExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".svg"];
    private readonly string[] _permittedMimeTypes =
        ["image/jpeg", "image/png", "image/gif", "image/bmp", "image/tiff", "image/svg+xml"];

    [HttpPost("Upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            if (file == null)
            {
                return BadRequest("No file uploaded.");
            }

            if (!_permittedMimeTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest("Invalid file type.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_permittedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file extension.");
            }
            await fileRepository.UploadFileAsync(file);
            return Ok("Image Uploaded");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}