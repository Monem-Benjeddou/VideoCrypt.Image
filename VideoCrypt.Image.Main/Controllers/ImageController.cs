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
                return RedirectToAction("Index", "Gallery"); 
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", new { message = ex.Message });
            }
        }
    }
}