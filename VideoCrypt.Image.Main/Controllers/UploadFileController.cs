using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UploadFileController(IFileRepository fileRepository) : Controller
    {
        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                await fileRepository.UploadFileAsync(file);
                return Ok("Image Uploaded");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GenerateShareLink/{fileName}")]
        public async Task<IActionResult> GenerateShareLink(string fileName)
        {
            try
            {
                var fileUrl = await fileRepository.GenerateFileLink(fileName);
                if (string.IsNullOrEmpty(fileUrl)) return NotFound("File share link is not found");
                return Ok(fileUrl);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}