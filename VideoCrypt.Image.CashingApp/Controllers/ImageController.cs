using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VideoCrypt.Image.CashingApp.Repository;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.CashingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImageController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;

        public ImageController(IImageRepository imageRepository)
        {
            _imageRepository = imageRepository;
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetImageUrl(string fileName)
        {
            try
            {
                var url = await _imageRepository.GetSharedFileUrlAsync(fileName);
                return Ok(new { Url = url });
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
                var paginatedList = await _imageRepository.ListImagesAsync(page, pageSize);
                return Ok(paginatedList);
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
                var deletedFromCache = await _imageRepository.DeleteCachedFileAsync(fileName);

                var deletedFromMinio = await _imageRepository.DeleteFileFromBucketAsync(fileName);

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
                Console.WriteLine($"Error encountered: '{ex.Message}' when deleting file");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
