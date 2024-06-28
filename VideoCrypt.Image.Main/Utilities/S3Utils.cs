using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace VideoCrypt.Image.Main.Utils
{
    public static class S3Utils
    {
        /* const string ServiceUrl = "http://192.168.30.23:9000";
         const string AccessKey = "Qqt3KMXNlK4iCKqPhgEd";
         const string SecretKey = "Kncx7QKlHyaN1rmbRRrAqDvDLGhGt8IAPdwhyjg6";
         const string SourceBucket = "imagesbucket";
         const string DestinationBucket = "public";*/
        
         private const string ServiceUrl = "http://10.13.111.3:9000";
         const string AccessKey = "Qqt3KMXNlK4iCKqPhgEd";
         const string SecretKey = "Kncx7QKlHyaN1rmbRRrAqDvDLGhGt8IAPdwhyjg6";
         public const string SourceBucket = "imagesbucket";
        private static AmazonS3Client GetS3Client()
        {
            var config = new AmazonS3Config
            {
                ServiceURL = ServiceUrl,
                ForcePathStyle = true
            };

            var credentials = new BasicAWSCredentials(AccessKey, SecretKey);
            return new AmazonS3Client(credentials, config);
        }

        public static async Task UploadFileAsync(string fileName, MemoryStream memoryStream, string bucketName, string contentType)
        {
            try
            {
                var client = GetS3Client();

                memoryStream.Seek(0, SeekOrigin.Begin); // Ensure stream is at the beginning
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName,
                    InputStream = memoryStream,
                    ContentType = contentType
                };

                var response = await client.PutObjectAsync(request);

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Error uploading file: {response.HttpStatusCode}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error encountered: '{e.Message}' when uploading an object");
                throw;
            }
        }

        public static async Task<byte[]> DownloadFileAsync(string fileName, string bucketName)
        {
            try
            {
                var client = GetS3Client();

                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = fileName
                };

                using (var response = await client.GetObjectAsync(request))
                {
                    if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception($"Error downloading file: {response.HttpStatusCode}");
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await response.ResponseStream.CopyToAsync(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error encountered: '{e.Message}' when downloading an object");
                throw;
            }
        }

        public static async Task<List<S3Bucket>> ListBucketsAsync()
        {
            try
            {
                var client = GetS3Client();
                var response = await client.ListBucketsAsync();

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Error listing buckets: {response.HttpStatusCode}");
                }

                return response.Buckets;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error encountered: '{e.Message}' when listing buckets");
                throw;
            }
        }
    }
}
