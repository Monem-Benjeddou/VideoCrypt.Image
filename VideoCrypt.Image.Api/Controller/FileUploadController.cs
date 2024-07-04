using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Main.Utils;

namespace VideoCrypt.Image.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("File is null.");

                var fileName = Path.GetFileName(file.FileName);
                if (await S3Utils.FileExistsAsync(fileName))
                {
                    return Conflict("File already exists in the bucket.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    await S3Utils.UploadFileAsync(fileName, memoryStream, file.ContentType);
                }

                return Ok("File uploaded successfully.");
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
                var fileBytes = await S3Utils.DownloadFileAsync(fileName, S3Utils.SourceBucket);
                return File(fileBytes, "image/jpeg");
            }
            catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("Image not found in the bucket.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListFiles()
        {
            try
            {
                var files = await S3Utils.ListFilesAsync();
                return Ok(files);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
