using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UploadFileController(IFileRepository fileRepository) : Microsoft.AspNetCore.Mvc.Controller
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
    }
}