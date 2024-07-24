using System.Globalization;using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Api.Repositories;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController(
        IImageRepository imageRepository,
        UserManager<IdentityUser> userManager
    ) : ControllerBase
    {
        private readonly IImageRepository _imageRepository =
            imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));

        private readonly string _baseUrl = "https://image.john-group.org";

        private readonly UserManager<IdentityUser> _userManager =
            userManager ?? throw new ArgumentNullException(nameof(userManager));

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile? file)
        {
            try
            {
                if (file == null)
                    return BadRequest("File is null.");
                var imageResponse = await _imageRepository.UploadFileAsync(file, User);

                return Ok(imageResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteImage(string fileName)
        {
            try
            {
                await _imageRepository.DeleteAsync(fileName, User);
                return Ok($"File '{fileName}' deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("image/{fileName}")]
        public async Task<IActionResult> GetImage(string fileName)
        {
            try
            {
                var url = await _imageRepository.GetImageAsync(fileName, User);
                return Ok(url);
            }
            catch (CultureNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound(ex.Message);
            }
            catch (HttpProtocolException ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode((int)ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                var fileBytes = await _imageRepository.DownloadFileAsync(fileName, User);
                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListFiles(int page = 1, int pageSize = 10)
        {
            try
            {
                var files = await _imageRepository.ListImagesAsync(page, pageSize, User);
                return Ok(files);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("getResized")]
        public async Task<IActionResult> GetResizedImage([FromQuery] string fileName, [FromQuery] int width,
            [FromQuery] int height, [FromQuery] ImageModificationType type)
        {
            try
            {
                var resizedImageUrl = await _imageRepository.ResizeImageAsync(fileName, width, height, type, User);
                return Ok(resizedImageUrl);
            }
            catch (CultureNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound("The specified image not found");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}