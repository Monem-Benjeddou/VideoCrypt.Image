using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Dapper;
using VideoCrypt.Image.Data;
using VideoCrypt.Image.Data.Models;
using VideoCrypt.Image.Server.Utilities;

namespace VideoCrypt.Image.CashingApp.Repository
{
    public class ImageRepository : IImageRepository
    {
        private readonly string _serviceUrl;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private const string _serverPort = ":9000";
        private readonly ApplicationDbContext _context;
        private readonly AmazonS3Client _s3Client;
        private readonly ILogger<ImageRepository> _logger;

        private const string InsertSql =
            @"INSERT INTO image_metadata (file_name, cached_file_path, url, created_at, user_id) VALUES (@FileName, @CachedFilePath, @Url, @CreatedAt, @UserId)";

        private const string SelectByNameSql =
            @"SELECT * FROM image_metadata WHERE file_name = @FileName AND user_id = @UserId";

        public ImageRepository(ApplicationDbContext context, ILogger<ImageRepository> logger)
        {
            _secretKey = Environment.GetEnvironmentVariable("secret_key") ??
                         throw new Exception("Secret key not found");
            _accessKey = Environment.GetEnvironmentVariable("access_key") ??
                         throw new Exception("Access key not found");
            _serviceUrl = Environment.GetEnvironmentVariable("service_url") ??
                          throw new Exception("Service url not found");
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var config = new AmazonS3Config
            {
                ServiceURL = _serviceUrl + _serverPort,
                ForcePathStyle = true
            };

            var credentials = new BasicAWSCredentials(_accessKey, _secretKey);
            _s3Client = new AmazonS3Client(credentials, config);
        }

        public async Task<string> GetSharedFileUrlAsync(string fileName, string userId, int height = 0, int width = 0,
            ImageModificationType type = ImageModificationType.Resize)
        {
            _logger.LogInformation($"Attempting to retrieve URL for file: {fileName} for user: {userId}");
            var modifiedName = "";
            if (height != 0 && width != 0)
                modifiedName = GenerateModifiedFileName(fileName, width, height, type);

            using (var connection = _context.CreateConnection())
            {
                var searchedName = string.IsNullOrEmpty(modifiedName) ? fileName : modifiedName;
                var cachedImage =
                    await connection.QueryFirstOrDefaultAsync<ImageMetadata>(SelectByNameSql,
                        new { FileName = searchedName, UserId = userId });

                if (cachedImage != null)
                {
                    _logger.LogInformation($"File {fileName} found in cache with URL: {cachedImage.Url}");
                    return cachedImage.Url;
                }
            }

            try
            {
                var userBucket = await GetOrCreateUserBucketAsync(userId);
                var fileBytes = await DownloadFileAsync(fileName, userBucket);
                if (height != 0 && width != 0)
                {
                    fileBytes = ResizeImageUtility.ResizeImage(fileBytes, width, height, type);
                    fileName = modifiedName;
                }

                var cacheDirectory = Path.Combine("/app/cache", userId);

                EnsureCacheDirectoryExists(cacheDirectory);

                var cacheFilePath = Path.Combine(cacheDirectory, fileName);
                await File.WriteAllBytesAsync(cacheFilePath, fileBytes);
                _logger.LogInformation($"File {fileName} cached at: {cacheFilePath}");

                var url = GenerateCachedFileUrl(fileName, userId);

                var newImageMetadata = new ImageMetadata
                {
                    FileName = fileName,
                    CachedFilePath = cacheFilePath,
                    Url = url,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                using (var dbConnection = _context.CreateConnection())
                {
                    await dbConnection.ExecuteAsync(InsertSql, newImageMetadata);
                }

                _logger.LogInformation($"Metadata for {fileName} saved in database with URL: {url}");

                return url;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError($"Error encountered: '{ex.Message}' when fetching or caching file");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error encountered: '{e.Message}' when fetching or caching file");
                throw;
            }
        }

        public async Task<PaginatedList<string>> ListImagesAsync(string userId, int page, int pageSize,
            string searchQuery)
        {
            try
            {
                _logger.LogInformation(
                    $"Listing images for user: {userId}, Page: {page}, PageSize: {pageSize}, SearchQuery: {searchQuery}");

                var userBucket = await GetOrCreateUserBucketAsync(userId);
                var request = new ListObjectsV2Request
                {
                    BucketName = userBucket,
                    
                };

                var response = await _s3Client.ListObjectsV2Async(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError($"Error listing files: {response.HttpStatusCode}");
                    throw new Exception($"Error listing files: {response.HttpStatusCode}");
                }

                var filteredObjects = response.S3Objects
                    .Where(x => string.IsNullOrEmpty(searchQuery) ||
                            x.Key.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.LastModified)
                    .ToList();

                var totalCount = filteredObjects.Count;
                _logger.LogInformation($"Total files in bucket matching query: {totalCount}");

                List<string> fileList;
                var cacheDirectory = Path.Combine("/app/cache", userId);

                EnsureCacheDirectoryExists(cacheDirectory);

                var imageMetadataList = new List<ImageMetadata>();

                using (var connection = _context.CreateConnection())
                {
                    var objects = filteredObjects
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Distinct()
                        .ToList();

                    foreach (var s3Object in objects)
                    {
                        _logger.LogInformation($"Processing file: {s3Object.Key}");

                        var cachedImage = await connection.QueryFirstOrDefaultAsync<ImageMetadata>(SelectByNameSql,
                            new { FileName = s3Object.Key, UserId = userId });

                        if (cachedImage != null)
                        {
                            _logger.LogInformation($"Found cached metadata for file: {s3Object.Key}");
                            imageMetadataList.Add(cachedImage);
                        }
                        else
                        {
                            var cacheFilePath = Path.Combine(cacheDirectory, s3Object.Key);

                            if (!File.Exists(cacheFilePath))
                            {
                                _logger.LogInformation($"Downloading and caching file: {s3Object.Key}");
                                var fileBytes = await DownloadFileAsync(s3Object.Key, userBucket);
                                await File.WriteAllBytesAsync(cacheFilePath, fileBytes);
                            }
                            else
                            {
                                _logger.LogInformation($"File already cached: {s3Object.Key}");
                            }

                            var url = GenerateCachedFileUrl(s3Object.Key, userId);

                            var newImageMetadata = new ImageMetadata
                            {
                                FileName = s3Object.Key,
                                CachedFilePath = cacheFilePath,
                                Url = url,
                                CreatedAt = s3Object.LastModified,
                                UserId = userId
                            };

                            await connection.ExecuteAsync(InsertSql, newImageMetadata);
                            imageMetadataList.Add(newImageMetadata);
                        }
                    }
                }

                var orderedImageMetadataList = imageMetadataList
                    .OrderByDescending(im => im.CreatedAt)
                    .ThenBy(im => im.FileName)
                    .ToList();
                fileList = orderedImageMetadataList.Select(im => im.Url).ToList();

                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                _logger.LogInformation($"Returning {fileList.Count} files. Page {page} of {totalPages}.");

                return new PaginatedList<string>
                {
                    Items = fileList,
                    PageIndex = page,
                    TotalPages = totalPages,
                    SearchQuery = searchQuery
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Error encountered: '{e.Message}' when listing files");
                throw;
            }
        }

        public async Task<bool> DeleteFileFromBucketAsync(string fileName, string userId)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete file: {fileName} for user: {userId} from bucket");

                var userBucket = await GetOrCreateUserBucketAsync(userId);
                var request = new DeleteObjectRequest
                {
                    BucketName = userBucket,
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

        public async Task<bool> DeleteCachedFileAsync(string fileName, string userId)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete cached file: {fileName} for user: {userId}");

                using var connection = _context.CreateConnection();
                var cachedImage = await connection.QueryFirstOrDefaultAsync<ImageMetadata>(SelectByNameSql,
                    new { FileName = fileName, UserId = userId });

                if (cachedImage != null)
                {
                    var deleteSql = "DELETE FROM image_metadata WHERE file_name = @FileName AND user_id = @UserId";
                    await connection.ExecuteAsync(deleteSql, new { FileName = fileName, UserId = userId });
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

        public async Task<bool> PurgeCacheAsync(string userId)
        {
            try
            {
                _logger.LogInformation($"Attempting to purge cache for user: {userId}");

                var cacheDirectory = Path.Combine("/app/cache", userId);

                if (Directory.Exists(cacheDirectory))
                {
                    Directory.Delete(cacheDirectory, true);
                    _logger.LogInformation($"Cache directory {cacheDirectory} deleted.");
                }

                using var connection = _context.CreateConnection();
                var deleteSql = "DELETE FROM image_metadata WHERE user_id = @UserId";
                await connection.ExecuteAsync(deleteSql, new { UserId = userId });

                _logger.LogInformation($"Metadata for user {userId} deleted from database.");

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error encountered: '{e.Message}' when purging cache.");
                throw;
            }
        }

        private string GenerateModifiedFileName(string originalFileName, int width, int height,
            ImageModificationType type)
        {
            var fileExtension = Path.GetExtension(originalFileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var typeString = type.ToString().ToLower();

            return $"{fileNameWithoutExtension}_{typeString}_w{width}_h{height}{fileExtension}";
        }

        private string GenerateCachedFileUrl(string fileName, string userId)
        {
            var url = $"https://image.john-group.org/cache/{userId}/{fileName}";
            _logger.LogInformation($"Generated URL for cached file: {url}");
            return url;
        }

        private void EnsureCacheDirectoryExists(string cacheDirectory)
        {
            if (Directory.Exists(cacheDirectory)) return;
            _logger.LogInformation($@"Creating cache directory: {cacheDirectory}");
            Directory.CreateDirectory(cacheDirectory);
        }

        private async Task<string> GetOrCreateUserBucketAsync(string userId)
        {
            var userBucket = $"{userId}";
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, userBucket);

            if (!bucketExists)
            {
                _logger.LogInformation($"Creating bucket for user: {userId}");

                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = userBucket
                };

                await _s3Client.PutBucketAsync(putBucketRequest);
                _logger.LogInformation($"Bucket {userBucket} created for user: {userId}");
            }

            return userBucket;
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
    }
}