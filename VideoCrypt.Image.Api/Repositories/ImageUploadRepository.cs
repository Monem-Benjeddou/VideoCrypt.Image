using System.Globalization;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace VideoCrypt.Image.Api.Utilities;

public class ImageUploadRepository : IImageUploadRepository
{
    private string _serviceUrl = Environment.GetEnvironmentVariable("service_url") ??
                                 throw new Exception("Service url  key not found");
    public const string UiPort = ":9001"; 
    const string _serverPort = ":9000";
    private string _accessKey = Environment.GetEnvironmentVariable("access_key") ?? 
                                throw new Exception("Access key not found");
    private string _secretKey = Environment.GetEnvironmentVariable("secret_key") ??
                                throw new Exception("Secret key not found"); 
    const string _sourceBucket = "imagesbucket";

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
    public async Task<bool> UploadFileAsync(string fileName, MemoryStream memoryStream, string contentType)
    {
        try
        {
            var client = GetS3Client();

            memoryStream.Seek(0, SeekOrigin.Begin);
            var request = new PutObjectRequest
            {
                BucketName = _sourceBucket,
                Key = fileName,
                InputStream = memoryStream,
                ContentType = contentType
            };

            var response = await client.PutObjectAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Error uploading file: {response.HttpStatusCode}");
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error encountered: '{e.Message}' when uploading an object");
            throw;
        }    
    }
    public async Task<bool> FileExistsAsync(string key)
    {
        try
        {
            var client = GetS3Client();
            var request = new GetObjectMetadataRequest
            {
                BucketName = _sourceBucket,
                Key = key
            };

            var response = await client.GetObjectMetadataAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;
            else
                throw;
        }
    }
    
}