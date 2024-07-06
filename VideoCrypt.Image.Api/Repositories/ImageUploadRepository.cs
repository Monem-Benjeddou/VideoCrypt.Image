using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace VideoCrypt.Image.Api.Utilities;

public class ImageUploadRepository : IImageUploadRepository
{
    private const string ServiceUrl = "http://10.13.111.3";
    public const string UiPort = ":9001";
    public const string ServerPort = ":9000";
    const string AccessKey = "Qqt3KMXNlK4iCKqPhgEd";
    const string SecretKey = "Kncx7QKlHyaN1rmbRRrAqDvDLGhGt8IAPdwhyjg6";
    public const string SourceBucket = "imagesbucket";
    private static AmazonS3Client GetS3Client()
    {
        var config = new AmazonS3Config
        {
            ServiceURL = ServiceUrl + ServerPort,
            ForcePathStyle = true
        };

        var credentials = new BasicAWSCredentials(AccessKey, SecretKey);
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
                BucketName = SourceBucket,
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
                BucketName = SourceBucket,
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