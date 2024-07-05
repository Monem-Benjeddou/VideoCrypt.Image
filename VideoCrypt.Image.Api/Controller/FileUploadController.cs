using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Main.Utils;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VideoCrypt.Image.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory; 
        private string _baseUrl = "http://51.38.80.38:4000";
        //private string _baseUrl = "http://localhost:4000";

        public FileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }
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
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromForm] IFormFile file)
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
                using (var client = _httpClientFactory.CreateClient())
                {
                    var response = await client.DeleteAsync($"{_baseUrl}:4000/api/file/delete/{fileName}");

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                            return NotFound("File not found.");

                        return StatusCode((int)response.StatusCode, $"Failed to download file: {response.ReasonPhrase}");
                    }

                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    return File(fileBytes, "application/octet-stream", fileName);
                }
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
                using (var client = _httpClientFactory.CreateClient())
                {
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
                using var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{_baseUrl}/api/Image/list?page={page}&pageSize={pageSize}");

                if (!response.IsSuccessStatusCode)
                {
                    return response.StatusCode == HttpStatusCode.NotFound ?
                        NotFound("No files found.") : 
                        StatusCode((int)response.StatusCode, $"Failed to get files: {response.ReasonPhrase}");
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var files = JsonSerializer.Deserialize<List<string>>(responseBody);

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
