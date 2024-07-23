using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VideoCrypt.Image.CashingApp.Repository;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.CashingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImageController(IImageRepository imageRepository) : ControllerBase
    {
        private readonly IImageRepository _imageRepository =
            imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetImageUrl(string fileName)
        {
            try
            {
                var url = await _imageRepository.GetSharedFileUrlAsync(fileName, GetUserId());
                return Ok(url);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("resize")]
        public async Task<IActionResult> ResizeImage([FromQuery] string fileName, [FromQuery] int width,
            [FromQuery] int height, [FromQuery] ImageModificationType type)
        {
            try
            {
                var url = await _imageRepository.GetSharedFileUrlAsync(fileName, GetUserId(), height, width, type);
                return Ok(url);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListFiles(int page = 1, int pageSize = 10)
        {
            try
            {
                var paginatedList = await _imageRepository.ListImagesAsync(GetUserId(), page, pageSize);
                return Ok(paginatedList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteImage(string fileName)
        {
            try
            {
                var deletedFromCache = await _imageRepository.DeleteCachedFileAsync(fileName, GetUserId());

                var deletedFromMinio = await _imageRepository.DeleteFileFromBucketAsync(fileName, GetUserId());

                if (deletedFromCache && deletedFromMinio)
                {
                    return Ok($"File '{fileName}' deleted successfully from cache and bucket.");
                }
                else
                {
                    return NotFound($"File '{fileName}' not found in cache or bucket.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string GetUserId()
        {
            if (!Request.Headers.TryGetValue("X-UserId", out var userIdHeader))
            {
                return null;
            }
            return userIdHeader;
        }
    }
}