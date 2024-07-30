using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Dashboard.Models;
using VideoCrypt.Image.Dashboard.Repositories;

namespace VideoCrypt.Image.Dashboard.Controllers;

[Authorize]
[Route("[controller]")]
public class ImageController(IFileRepository _fileRepository,HttpClient httpClient) : Controller
{
    [HttpPost("DeleteImage")]
    public async Task<IActionResult> DeleteImage(string imageUrl)
    {
        try
        {
            var fileName = imageUrl.Split('/').Last();
            await _fileRepository.DeleteFileAsync(fileName);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // Handle the exception and return an error view or message
            ViewBag.ErrorMessage = ex.Message;
            return View("Error");
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
    
    public async Task<IActionResult> Index(int page = 1,int pageSize = 8, string searchQuery = "")
    {
        var images = await _fileRepository.ListFilesAsync(page, pageSize, searchQuery);
        var viewModel = new PaginatedList<string>()
        {
            TotalPages = images.TotalPages,
            PageIndex = page,
            Items = images.Items,
            SearchQuery = searchQuery
        };
        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Search(string searchQuery)
    {
        return RedirectToAction("Index", new { searchQuery });
    }
}