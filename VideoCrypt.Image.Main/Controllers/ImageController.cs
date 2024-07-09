using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Controllers
{
    [Authorize]
    [ApiController]
    [Route("Image")]
    public class ImageController : Controller
    {
        private readonly IFileRepository _fileRepository;

        public ImageController(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        [HttpPost("deleteImage")]
        public async Task<IActionResult> DeleteImage([FromForm] string imageUrl)
        {
            try
            {
                var fileName = imageUrl.Split('/').Last();
                await _fileRepository.DeleteFileAsync(fileName);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet("GenerateShareLink/{fileName}")]
        public async Task<IActionResult> GenerateShareLink(string fileName)
        {
            try
            {
                var fileUrl = await _fileRepository.GenerateFileLink(fileName);
                if (string.IsNullOrEmpty(fileUrl)) return NotFound("File share link is not found");

                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}