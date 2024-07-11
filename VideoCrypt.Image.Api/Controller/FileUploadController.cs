using System.Net;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using VideoCrypt.Image.Api.Repositories;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Controller;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ImageController(
    IHttpClientFactory httpClientFactory,
    ApplicationDbContext context,
    IImageUploadRepository imageUploadRepository)
    : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory = 
        httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    private string _baseUrl = "image.john-group.org";

    //private string _baseUrl = "http://localhost:4000";

    private HttpClient CreateAuthorizedClient() => _httpClientFactory.CreateClient("AuthorizedClient");

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        try
        {
            if (file == null)
                return BadRequest("File is null.");
            await imageUploadRepository.UploadFileAsync(file);

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
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await SignOutUser();
                return Unauthorized("Unauthorized access. Please sign in again.");
            }
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
                return Ok(cachedImage.Url);

            using var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"{_baseUrl}/api/image/{fileName}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await SignOutUser();
                return Unauthorized("Unauthorized access. Please sign in again.");
            }

            if (response.IsSuccessStatusCode)
            {
                var imageUrl = await response.Content.ReadAsStringAsync();
                return Ok(imageUrl);
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
            var response = await client.GetAsync($"{_baseUrl}/api/file/download/{fileName}");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await SignOutUser();
                return Unauthorized("Unauthorized access. Please sign in again.");
            }

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
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await SignOutUser();
                return Unauthorized("Unauthorized access. Please sign in again.");
            }

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
    private async Task SignOutUser()
    {
        await HttpContext.SignOutAsync();
    }
}