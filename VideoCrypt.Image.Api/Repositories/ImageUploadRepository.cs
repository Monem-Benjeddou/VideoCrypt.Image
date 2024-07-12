using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VideoCrypt.Image.Api.Repositories
{
    public class ImageUploadRepository(ILogger<ImageUploadRepository> logger) : IImageUploadRepository
    {
        private readonly string _serviceUrl = Environment.GetEnvironmentVariable("service_url") ??
                                 throw new Exception("Service url key not found");
        private const string UiPort = ":9001"; 
        private const string _serverPort = ":9000";
        private readonly string _accessKey = Environment.GetEnvironmentVariable("access_key") ?? 
                                throw new Exception("Access key not found");
        private readonly string _secretKey = Environment.GetEnvironmentVariable("secret_key") ??
                                throw new Exception("Secret key not found"); 
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".svg" };
        private readonly string[] _permittedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/tiff", "image/svg+xml" };

        private readonly ILogger<ImageUploadRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        private AmazonS3Client GetS3Client()
        {
            var config = new AmazonS3Config
            {
                ServiceURL = _serviceUrl + _serverPort,
                ForcePathStyle = true
            };

            var credentials = new BasicAWSCredentials(_accessKey, _secretKey);
            return new AmazonS3Client(credentials, config);
        }

        public async Task<bool> UploadFileAsync(IFormFile formFile, string userId)
        {
            var userBucket = await GetUserBucketAsync(userId);

            if (formFile == null)
                throw new ArgumentException("formFile cannot be null", nameof(formFile));

            var fileName = formFile.FileName;
            var contentType = formFile.ContentType;
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!_permittedExtensions.Contains(fileExtension) || !_permittedMimeTypes.Contains(contentType))
            {
                _logger.LogWarning("File extension {FileExtension} or MIME type {ContentType} is not permitted", fileExtension, contentType);
                throw new InvalidOperationException("File type is not permitted.");
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await formFile.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var client = GetS3Client();
                var request = new PutObjectRequest
                {
                    BucketName = userBucket,
                    Key = fileName,
                    InputStream = memoryStream,
                    ContentType = contentType
                };

                _logger.LogInformation("Uploading file {FileName} to bucket {BucketName}", fileName, userBucket);
                var response = await client.PutObjectAsync(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("Error uploading file {FileName}: {HttpStatusCode}", fileName, response.HttpStatusCode);
                    throw new Exception($"Error uploading file: {response.HttpStatusCode}");
                }

                _logger.LogInformation("File {FileName} uploaded successfully", fileName);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error encountered when uploading an object");
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string key, string userId)
        {
            try
            {
                var userBucket = await GetUserBucketAsync(userId);

                var client = GetS3Client();
                var request = new ListObjectsRequest
                {
                    BucketName = userBucket,
                    Prefix = key,
                    MaxKeys = 1
                };

                _logger.LogInformation("Checking if file {Key} exists in bucket {BucketName}", key, userBucket);
                var response = await client.ListObjectsAsync(request, CancellationToken.None);

                var fileExists = response.S3Objects.Any();
                _logger.LogInformation("File {Key} exists: {FileExists}", key, fileExists);
                return fileExists;
            }
            catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("File {Key} not found in bucket {BucketName}", key);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if file exists: {Key}", key);
                throw;
            }
        }

        private async Task<string> GetUserBucketAsync(string userId)
        {
            var bucketPrefix = $"{userId}-imagesbucket";
            var _s3Client = GetS3Client();
            var listBucketsResponse = await _s3Client.ListBucketsAsync();

            var userBucket = listBucketsResponse.Buckets
                .FirstOrDefault(b => b.BucketName.StartsWith(bucketPrefix));

            if (userBucket != null) return userBucket.BucketName;
            _logger.LogInformation("Bucket for user {UserId} does not exist. Creating a new one.", userId);

            var createBucketRequest = new PutBucketRequest
            {
                BucketName = bucketPrefix
            };

            var createBucketResponse = await _s3Client.PutBucketAsync(createBucketRequest);

            if (createBucketResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError("Error creating bucket {BucketName}: {HttpStatusCode}", bucketPrefix, createBucketResponse.HttpStatusCode);
                throw new Exception($"Error creating bucket: {createBucketResponse.HttpStatusCode}");
            }

            userBucket = new S3Bucket { BucketName = bucketPrefix };
            _logger.LogInformation("Bucket {BucketName} created successfully", bucketPrefix);

            return userBucket.BucketName;
        }
    }
}
