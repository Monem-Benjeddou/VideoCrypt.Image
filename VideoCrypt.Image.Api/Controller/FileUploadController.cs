using System.Net;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Api.Utilities;
using VideoCrypt.Image.Data;
using System.Text.Json;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FileController(
        IHttpClientFactory httpClientFactory,
        ApplicationDbContext context,
        IImageUploadRepository imageUploadRepository)
        : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory = 
            httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

        private string _baseUrl = "http://51.38.80.38:4000";

        //private string _baseUrl = "http://localhost:4000";

        private HttpClient CreateAuthorizedClient()
        {
            return _httpClientFactory.CreateClient("AuthorizedClient");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            try
            {
                if (file == null)
                    return BadRequest("File is null.");

                var fileName = Path.GetFileName(file.FileName);
                if (await imageUploadRepository.FileExistsAsync(fileName))
                {
                    return Conflict("File already exists in the bucket.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    await imageUploadRepository.UploadFileAsync(fileName, memoryStream, file.ContentType);
                }

                return Ok("File uploaded successfully.");
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
                using var client = CreateAuthorizedClient();
                var response = await client.DeleteAsync($"{_baseUrl}/api/Image/{fileName}");

                if (response.IsSuccessStatusCode) return Ok($"File '{fileName}' deleted successfully.");
                return response.StatusCode == HttpStatusCode.NotFound
                    ? NotFound("File not found.")
                    : StatusCode((int)response.StatusCode, $"Failed to delete file: {response.ReasonPhrase}");
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
                using var connection = context.CreateConnection();
                var cachedImage = await connection.QueryFirstOrDefaultAsync<ImageMetadata>(
                    "SELECT * FROM image_metadata WHERE file_name = @FileName", new { FileName = fileName });

                if (cachedImage != null)
                    return Ok(new { Url = cachedImage.Url });

                using var client = CreateAuthorizedClient();
                var response = await client.GetAsync($"{_baseUrl}/api/file/image/{fileName}");

                if (response.IsSuccessStatusCode)
                {
                    var imageUrl = await response.Content.ReadAsStringAsync();
                    return Ok(new { Url = imageUrl });
                }

                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound => NotFound("Image not found."),
                    _ => StatusCode((int)response.StatusCode, $"Failed to retrieve image: {response.ReasonPhrase}")
                };
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
                using var client = CreateAuthorizedClient();
                var response = await client.GetAsync($"{_baseUrl}:4000/api/file/download/{fileName}");
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return NotFound("File not found.");

                    return StatusCode((int)response.StatusCode, $"Failed to download file: {response.ReasonPhrase}");
                }

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
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
                using var client = CreateAuthorizedClient();
                var response = await client.GetAsync($"{_baseUrl}/api/Image/list?page={page}&pageSize={pageSize}");

                if (!response.IsSuccessStatusCode)
                {
                    return response.StatusCode == HttpStatusCode.NotFound
                        ? NotFound("No files found.")
                        : StatusCode((int)response.StatusCode, $"Failed to get files: {response.ReasonPhrase}");
                }
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var responseBody = await response.Content.ReadAsStringAsync();
                var files = JsonSerializer.Deserialize<PaginatedList<string>>(responseBody,options);
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