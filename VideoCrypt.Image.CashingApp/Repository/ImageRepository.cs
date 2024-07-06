using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VideoCrypt.Image.Data;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.CashingApp.Repository
{
    public class ImageRepository : IImageRepository
    {
        private const string _serviceUrl = "http://10.13.111.3";
        private const string _serverPort = ":9000";
        private const string _accessKey = "Qqt3KMXNlK4iCKqPhgEd";
        private const string _secretKey = "Kncx7QKlHyaN1rmbRRrAqDvDLGhGt8IAPdwhyjg6";
        private const string _sourceBucket = "imagesbucket";

        private readonly ApplicationDbContext _context;
        private readonly AmazonS3Client _s3Client;
        private readonly ILogger<ImageRepository> _logger;

        public ImageRepository(ApplicationDbContext context, ILogger<ImageRepository> logger)
        {
            _context = context;
            _logger = logger;

            var config = new AmazonS3Config
            {
                ServiceURL = _serviceUrl + _serverPort,
                ForcePathStyle = true
            };

            var credentials = new BasicAWSCredentials(_accessKey, _secretKey);
            _s3Client = new AmazonS3Client(credentials, config);
        }

        public async Task<string> GetSharedFileUrlAsync(string fileName)
        {
            _logger.LogInformation($"Attempting to retrieve URL for file: {fileName}");

            var cachedImage = await _context.ImageMetadata.FirstOrDefaultAsync(i => i.FileName == fileName);
            if (cachedImage != null)
            {
                _logger.LogInformation($"File {fileName} found in cache with URL: {cachedImage.Url}");
                return cachedImage.Url;
            }

            try
            {
                var fileBytes = await DownloadFileAsync(fileName, _sourceBucket);
                var cacheDirectory = Path.Combine("/app/cache");

                if (!Directory.Exists(cacheDirectory))
                {
                    _logger.LogInformation($"Creating cache directory: {cacheDirectory}");
                    Directory.CreateDirectory(cacheDirectory);
                }

                var cacheFilePath = Path.Combine(cacheDirectory, fileName);
                await File.WriteAllBytesAsync(cacheFilePath, fileBytes);
                _logger.LogInformation($"File {fileName} cached at: {cacheFilePath}");

                var url = GenerateCachedFileUrl(fileName);

                var newImageMetadata = new ImageMetadata
                {
                    FileName = fileName,
                    CachedFilePath = cacheFilePath,
                    Url = url,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ImageMetadata.Add(newImageMetadata);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Metadata for {fileName} saved in database with URL: {url}");

                return url;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error encountered: '{e.Message}' when fetching or caching file");
                throw;
            }
        }

        private async Task<byte[]> DownloadFileAsync(string fileName, string bucketName)
        {
            try
            {
                _logger.LogInformation($"Attempting to download file: {fileName} from bucket: {bucketName}");

                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName
                };

                using (var response = await _s3Client.GetObjectAsync(request))
                using (var memoryStream = new MemoryStream())
                {
                    await response.ResponseStream.CopyToAsync(memoryStream);
                    _logger.LogInformation($"File: {fileName} downloaded successfully");
                    return memoryStream.ToArray();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error encountered: '{e.Message}' when downloading an object");
                throw;
            }
        }

       public async Task<List<string>> ListImagesAsync(int page, int pageSize)
        {
            try
            {
                _logger.LogInformation($"Listing images, Page: {page}, PageSize: {pageSize}");

                var request = new ListObjectsV2Request
                {
                    BucketName = _sourceBucket,
                    MaxKeys = pageSize,
                    ContinuationToken = GetContinuationToken(page, pageSize)
                };

                var response = await _s3Client.ListObjectsV2Async(request);

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError($"Error listing files: {response.HttpStatusCode}");
                    throw new Exception($"Error listing files: {response.HttpStatusCode}");
                }

                var fileList = new List<string>();
                var cacheDirectory = Path.Combine("/app/cache");

                if (!Directory.Exists(cacheDirectory))
                {
                    Directory.CreateDirectory(cacheDirectory);
                }

                var imageMetadataList = new List<ImageMetadata>();

                using (var connection = _context.CreateConnection())
                {
                    foreach (var s3Object in response.S3Objects)
                    {
                        var cachedImage = await connection.QueryFirstOrDefaultAsync<ImageMetadata>(
                            "SELECT * FROM ImageMetadata WHERE FileName = @FileName", new { FileName = s3Object.Key });

                        if (cachedImage != null)
                        {
                            imageMetadataList.Add(cachedImage);
                        }
                        else
                        {
                            var fileBytes = await DownloadFileAsync(s3Object.Key, _sourceBucket);
                            var cacheFilePath = Path.Combine(cacheDirectory, s3Object.Key);

                            await File.WriteAllBytesAsync(cacheFilePath, fileBytes);

                            var url = GenerateCachedFileUrl(s3Object.Key);

                            var newImageMetadata = new ImageMetadata
                            {
                                FileName = s3Object.Key,
                                CachedFilePath = cacheFilePath,
                                Url = url,
                                CreatedAt = DateTime.UtcNow
                            };

                            var sql = "INSERT INTO ImageMetadata (FileName, CachedFilePath, Url, CreatedAt) VALUES (@FileName, @CachedFilePath, @Url, @CreatedAt)";
                            await connection.ExecuteAsync(sql, newImageMetadata);

                            imageMetadataList.Add(newImageMetadata);
                        }
                    }
                }

                var orderedImageMetadataList = imageMetadataList.OrderByDescending(im => im.CreatedAt).ToList();
                fileList = orderedImageMetadataList.Select(im => im.Url).ToList();

                return fileList;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error encountered: '{e.Message}' when listing files");
                throw;
            }
        }
        public async Task<bool> DeleteFileFromBucketAsync(string fileName)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete file: {fileName} from bucket");

                var request = new DeleteObjectRequest
                {
                    BucketName = _sourceBucket,
                    Key = fileName
                };
                var response = await _s3Client.DeleteObjectAsync(request);

                if (string.IsNullOrWhiteSpace(response.DeleteMarker)) return true;
                _logger.LogError($"Error deleting: {response.HttpStatusCode}");
                throw new Exception($"Error deleting: {response.HttpStatusCode}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error encountered: '{e.Message}' when deleting file.");
                throw;
            }
        }

        public async Task<bool> DeleteCachedFileAsync(string fileName)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete cached file: {fileName}");

                using var connection = _context.CreateConnection();
                var cachedImage = await connection.QueryFirstOrDefaultAsync<ImageMetadata>(
                    "SELECT * FROM ImageMetadata WHERE FileName = @FileName", new { FileName = fileName });

                if (cachedImage != null)
                {
                    var deleteSql = "DELETE FROM ImageMetadata WHERE FileName = @FileName";
                    await connection.ExecuteAsync(deleteSql, new { FileName = fileName });
                }

                var cacheFilePath = cachedImage?.CachedFilePath;
                if (cacheFilePath == null || !File.Exists(cacheFilePath)) return true;
                File.Delete(cacheFilePath);
                _logger.LogInformation($"Cached file {fileName} deleted from path: {cacheFilePath}");

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error encountered: '{e.Message}' when deleting cached file.");
                throw;
            }
        }

        private string GenerateCachedFileUrl(string fileName)
        {
            var url = $"http://51.38.80.38:4000/cache/{fileName}";
            _logger.LogInformation($"Generated URL for cached file: {url}");
            return url;
        }

        private string GetContinuationToken(int page, int pageSize)
        {
            if (page <= 1)
                return null;

            int skip = (page - 1) * pageSize;
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"skip={skip}"));
        }
    }
}