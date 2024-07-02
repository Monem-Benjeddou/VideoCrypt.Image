using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace VideoCrypt.Image.Main.Utils
{
    public static class S3Utils
    {
        /* const string ServiceUrl = "http://192.168.30.23:9000";
         const string AccessKey = "Qqt3KMXNlK4iCKqPhgEd";
         const string SecretKey = "Kncx7QKlHyaN1rmbRRrAqDvDLGhGt8IAPdwhyjg6";
         const string SourceBucket = "imagesbucket";
         const string DestinationBucket = "public";*/
        
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
        public static async Task<List<byte[]>> ListFilesAsync()
        {
            try
            {
                var client = GetS3Client();
                var request = new ListObjectsV2Request
                {
                    BucketName = SourceBucket
                };

                var response = await client.ListObjectsV2Async(request);

                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Error listing files: {response.HttpStatusCode}");
                }

                var fileBytesList = new List<byte[]>();
                foreach (var s3Object in response.S3Objects)
                {
                    var fileName = s3Object.Key;
                    var fileBytes = await DownloadFileAsync(fileName, SourceBucket);
                    fileBytesList.Add(fileBytes);
                }

                return fileBytesList;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error encountered: '{e.Message}' when listing files");
                throw;
            }
        }


        public static async Task UploadFileAsync(string fileName, MemoryStream memoryStream, string contentType)
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
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error encountered: '{e.Message}' when uploading an object");
                throw;
            }
        }
        public static async Task<bool> FileExistsAsync(string key)
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
        public static string GenerateCustomPreSignedUrl(string key, TimeSpan expiry)
        {
            var client = GetS3Client();
            var request = new GetPreSignedUrlRequest
            {
                BucketName = SourceBucket,
                Key = key,
                Expires = DateTime.UtcNow.Add(expiry)
            };

            string preSignedUrl = client.GetPreSignedURL(request);

            string customUrl = $"{ServiceUrl}{UiPort}/api/v1/buckets/{SourceBucket}/objects/download?preview=true&prefix={Uri.EscapeDataString(key)}&version_id=null";

            return customUrl;
        }

    }
}
