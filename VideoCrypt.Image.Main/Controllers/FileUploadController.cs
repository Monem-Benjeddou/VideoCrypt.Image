using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Minio.Exceptions;

namespace VideoCrypt.Image.Main.Controllers
{
    public class FileUploadController : Controller
    {
        private readonly IMinioClient _minioClient;
        private readonly string _cacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), "ImageCache");

        public FileUploadController(IMinioClient minioClient)
        {
            _minioClient = minioClient;
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
                var bucketName = "imagesbucket";

                // Check if the file already exists in the cache
                if (System.IO.File.Exists(filePath))
                {
                    return Ok("File already exists in the cache.");
                }

                var beArgs = new BucketExistsArgs().WithBucket(bucketName);
                var found = await _minioClient.BucketExistsAsync(beArgs);
                if (!found)
                {
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
                }

                // Read file into a memory stream
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin); // Reset the position to the beginning

                    // Upload file to Minio
                    var bucketObject = new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(fileName)
                        .WithStreamData(memoryStream)
                        .WithObjectSize(memoryStream.Length)
                        .WithContentType(file.ContentType);

                    await _minioClient.PutObjectAsync(bucketObject).ConfigureAwait(false);
                }


                return Ok("File uploaded and cached successfully.");
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

            var bucketName = "imagesbucket";

            try
            {
                byte[] fileBytes;

                using (var memoryStream = new MemoryStream())
                {
                    GetObjectArgs getObjectArgs = new GetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(fileName)
                        .WithCallbackStream((stream) => 
                        {
                            stream.CopyTo(memoryStream);
                        });

                    await _minioClient.GetObjectAsync(getObjectArgs);

                    memoryStream.Seek(0, SeekOrigin.Begin); 
                    fileBytes = memoryStream.ToArray(); 
                }

                // Save file to cache directory
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                // Return file from byte array
                return File(fileBytes, "image/jpeg"); 
            }
            catch (ObjectNotFoundException)
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
                var buckets = await _minioClient.ListBucketsAsync();
                return Ok("Connection successful. Buckets: " + string.Join(", ", buckets.Buckets.Select(b => b.Name)));
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