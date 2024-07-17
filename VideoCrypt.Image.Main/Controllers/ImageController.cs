using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Controllers
{
    [Authorize]
    [ApiController]
    [Route("Image")]
    public class ImageController(IFileRepository fileRepository,HttpClient httpClient) : Controller
    {
        [HttpPost("deleteImage")]
        public async Task<IActionResult> DeleteImage([FromForm] string imageUrl)
        {
            try
            {
                var fileName = imageUrl.Split('/').Last();
                await fileRepository.DeleteFileAsync(fileName);
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
                var fileUrl = await fileRepository.GenerateFileLink(fileName);
                if (string.IsNullOrEmpty(fileUrl)) return NotFound("File share link is not found");

                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("download")]
        public async Task<IActionResult> DownloadImage([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL is required");
            }

            try
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to download image");
                }

                var content = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType.ToString();
                var fileName = url.Substring(url.LastIndexOf('/') + 1);

                return File(content, contentType, fileName);
            }
            catch
            {
                return StatusCode(500, "An error occurred while downloading the image");
            }
        }
    }
}