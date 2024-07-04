using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using VideoCrypt.Image.Data;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Server.Repository;

public class ImageRepository : IImageRepository
{
    private const string ServiceUrl = "http://10.13.111.3";
    public const string ServerPort = ":9000";
    const string AccessKey = "Qqt3KMXNlK4iCKqPhgEd";
    const string SecretKey = "Kncx7QKlHyaN1rmbRRrAqDvDLGhGt8IAPdwhyjg6";
    public const string SourceBucket = "imagesbucket";

    private readonly ApplicationDbContext _context;
    private readonly AmazonS3Client _s3Client;

    public ImageRepository(ApplicationDbContext context)
    {
        _context = context;

        var config = new AmazonS3Config
        {
            ServiceURL = ServiceUrl + ServerPort,
            ForcePathStyle = true
        };

        var credentials = new BasicAWSCredentials(AccessKey, SecretKey);
        _s3Client = new AmazonS3Client(credentials, config);
    }

    public async Task<string> GetSharedFileUrlAsync(string fileName)
    {
        var cachedImage = await _context.ImageMetadata.FirstOrDefaultAsync(i => i.FileName == fileName);
        if (cachedImage != null)
        {
            return cachedImage.Url;
        }

        try
        {
            var fileBytes = await DownloadFileAsync(fileName, SourceBucket);
            var cacheFilePath = Path.Combine("/path/to/shared/cache/directory", fileName); 
            await File.WriteAllBytesAsync(cacheFilePath, fileBytes);

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

            return url;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error encountered: '{e.Message}' when fetching or caching file");
            throw;
        }
    }

    private async Task<byte[]> DownloadFileAsync(string fileName, string bucketName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            using (var response = await _s3Client.GetObjectAsync(request))
            using (var memoryStream = new MemoryStream())
            {
                await response.ResponseStream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error encountered: '{e.Message}' when downloading an object");
            throw;
        }
    }

    public async Task<List<string>> ListImagesAsync()
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = SourceBucket
            };

            var response = await _s3Client.ListObjectsV2Async(request);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Error listing files: {response.HttpStatusCode}");
            }

            var fileList = new List<string>();
            if (fileList == null) throw new ArgumentNullException(nameof(fileList));
            fileList.AddRange(response.S3Objects.Select(s3Object => s3Object.Key));

            return fileList;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error encountered: '{e.Message}' when listing files");
            throw;
        }
    }

    public async Task<bool> DeleteFileFromBucketAsync(string fileName)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = SourceBucket,
                Key = fileName
            };
            var response = await _s3Client.DeleteObjectAsync(request);
            
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Error deleting: {response.HttpStatusCode}");
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error encountered: '{e.Message}' when deleting file.");
            throw;
        }
        
    }

    public async Task<bool> DeleteCachedFileAsync(string fileName)
    {
        try
        {
            var cachedImage = await _context.ImageMetadata.FirstOrDefaultAsync(i => i.FileName == fileName);
            if (cachedImage != null)
            {
                _context.ImageMetadata.Remove(cachedImage);
                await _context.SaveChangesAsync();
            }

            var cacheFilePath = cachedImage?.CachedFilePath;
            if (cacheFilePath != null && File.Exists(cacheFilePath))
            {
                File.Delete(cacheFilePath);
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error encountered: '{e.Message}' when deleting cached file.");
            throw;
        }
    }


    private string GenerateCachedFileUrl(string fileName)
    {
        return $"http://your-server.com/cache/{fileName}"; // Change this to the actual URL format
    }
}