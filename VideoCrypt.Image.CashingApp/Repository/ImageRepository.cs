using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Dapper;
using Microsoft.Extensions.Logging;
using VideoCrypt.Image.Data;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.CashingApp.Repository
{
    public class ImageRepository : IImageRepository
    {
        private readonly string _serviceUrl;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private const string _serverPort = ":9000";
        private const string _sourceBucket = "imagesbucket";

        private readonly ApplicationDbContext _context;
        private readonly AmazonS3Client _s3Client;
        private readonly ILogger<ImageRepository> _logger;
        const string insertSql = @"INSERT INTO image_metadata (file_name, cached_file_path, url, created_at) VALUES (@FileName, @CachedFilePath, @Url, @CreatedAt)";
        const string selectByNameSql = @"SELECT * FROM image_metadata WHERE file_name = @FileName";
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

        public async Task<string> GetSharedFileUrlAsync(string fileName)
        {
            _logger.LogInformation($"Attempting to retrieve URL for file: {fileName}");

            using (var connection = _context.CreateConnection())
            {
                var cachedImage = await connection.QueryFirstOrDefaultAsync<ImageMetadata>(selectByNameSql, new { FileName = fileName });

                if (cachedImage != null)
                {
                    _logger.LogInformation($"File {fileName} found in cache with URL: {cachedImage.Url}");
                    return cachedImage.Url;
                }
            }

            try
            {
                var fileBytes = await DownloadFileAsync(fileName, _sourceBucket);
                var cacheDirectory = Path.Combine("/app/cache");

                EnsureCacheDirectoryExists(cacheDirectory);

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

                using (var dbConnection = _context.CreateConnection())
                {
                    await dbConnection.ExecuteAsync(insertSql, newImageMetadata);
                }

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

        public async Task<PaginatedList<string>> ListImagesAsync(int page, int pageSize)
        {
            try
            {
                _logger.LogInformation($"Listing images, Page: {page}, PageSize: {pageSize}");

                var request = new ListObjectsV2Request
                {
                    BucketName = _sourceBucket
                };

                var response = await _s3Client.ListObjectsV2Async(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError($"Error listing files: {response.HttpStatusCode}");
                    throw new Exception($"Error listing files: {response.HttpStatusCode}");
                }

                var totalCount = response.S3Objects.Count;
                _logger.LogInformation($"Total files in bucket: {totalCount}");

                List<string> fileList;
                var cacheDirectory = Path.Combine("/app/cache");

                EnsureCacheDirectoryExists(cacheDirectory);

                var imageMetadataList = new List<ImageMetadata>();

                using (var connection = _context.CreateConnection())
                {
                    var objects = response.S3Objects.OrderBy(x => x.LastModified)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Distinct()
                        .ToList();

                    foreach (var s3Object in objects)
                    {
                        _logger.LogInformation($"Processing file: {s3Object.Key}");

                        var cachedImage = await connection.QueryFirstOrDefaultAsync<ImageMetadata>(selectByNameSql,
                            new { FileName = s3Object.Key });

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
                                var fileBytes = await DownloadFileAsync(s3Object.Key, _sourceBucket);
                                await File.WriteAllBytesAsync(cacheFilePath, fileBytes);
                            }
                            else
                            {
                                _logger.LogInformation($"File already cached: {s3Object.Key}");
                            }

                            var url = GenerateCachedFileUrl(s3Object.Key);

                            var newImageMetadata = new ImageMetadata
                            {
                                FileName = s3Object.Key,
                                CachedFilePath = cacheFilePath,
                                Url = url,
                                CreatedAt = DateTime.UtcNow
                            };


                            await connection.ExecuteAsync(insertSql, newImageMetadata);

                            imageMetadataList.Add(newImageMetadata);
                        }
                    }
                }

                var orderedImageMetadataList = imageMetadataList.OrderByDescending(im => im.CreatedAt).ToList();
                fileList = orderedImageMetadataList.Select(im => im.Url).ToList();

                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                _logger.LogInformation($"Returning {fileList.Count} files. Page {page} of {totalPages}.");

                return new PaginatedList<string>
                {
                    Items = fileList,
                    PageIndex = page,
                    TotalPages = totalPages,
                    ContinuationToken = response.NextContinuationToken
                };
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
                var cachedImage = await connection.QueryFirstOrDefaultAsync<ImageMetadata>(selectByNameSql, new { FileName = fileName });

                if (cachedImage != null)
                {
                    var deleteSql = "DELETE FROM image_metadata WHERE file_name = @FileName";
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
            var url = $"https://image.john-group.org/cache/{fileName}";
            _logger.LogInformation($"Generated URL for cached file: {url}");
            return url;
        }

        private void EnsureCacheDirectoryExists(string cacheDirectory)
        {
            if (Directory.Exists(cacheDirectory)) return;
            _logger.LogInformation($@"Creating cache directory: {cacheDirectory}");
            Directory.CreateDirectory(cacheDirectory);
        }
    }
}
