using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoCrypt.Image.Main.Utils;

namespace VideoCrypt.Image.Main.Controllers
{
    public class FileUploadController : Controller
    {
        private readonly string _cacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ImageCache");

        public FileUploadController()
        {
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            if (file == null) return NoContent();

            try
            {
                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(_cacheDirectory, fileName);
                var bucketName = S3Utils.SourceBucket;

                if (System.IO.File.Exists(filePath))
                {
                    return Ok("File already exists in the cache.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin); // Reset the position to the beginning

                    await S3Utils.UploadFileAsync(fileName, memoryStream, bucketName, file.ContentType);

                }

                return Ok("File uploaded  successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetImage(string fileName)
        {
            var filePath = Path.Combine(_cacheDirectory, fileName);

            if (System.IO.File.Exists(filePath))
            {
                // Return file from cache
                var image = System.IO.File.OpenRead(filePath);
                return File(image, "image/jpeg"); // Adjust the content type based on your image type
            }

            var bucketName = S3Utils.SourceBucket;

            try
            {
                byte[] fileBytes = await S3Utils.DownloadFileAsync(fileName, bucketName);

                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                return File(fileBytes, "image/jpeg");
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound("Image not found in the bucket.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var buckets = await S3Utils.ListBucketsAsync();
                return Ok("Connection successful. Buckets: " + string.Join(", ", buckets.Select(b => b.BucketName)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Connection failed: {ex.Message}");
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
