using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using VideoCrypt.Image.Main.Models;
using Microsoft.Extensions.Logging;

namespace VideoCrypt.Image.Main.Repository
{
    public class FileRepository : IFileRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FileRepository> _logger;
        //private string _apiBaseUrl = "http://51.38.80.38:7003";
        private string _apiBaseUrl = "https://api.john-group.org";

        public FileRepository(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<FileRepository> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
        }

        public async Task UploadFileAsync(IFormFile? file)
        {
            using var content = new MultipartFormDataContent();
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var streamContent = new StreamContent(ms);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(streamContent, "file", file.FileName);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",  GetAccessToken());

            _logger.LogInformation("Uploading file {FileName} to {_apiBaseUrl}", file.FileName, _apiBaseUrl);
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/Image/upload", content);
            if (IsUnAuthorized(response))
            {
                throw new Exception("Unauthorized user please try logging out then logging int");
            }
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("File {FileName} uploaded successfully", file.FileName);
        }

        public async Task DeleteFileAsync(string fileName)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",  GetAccessToken());
            var uriFileName = Uri.EscapeUriString(fileName);

            _logger.LogInformation("Deleting file {FileName} from {_apiBaseUrl}", fileName, _apiBaseUrl);
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/Image/{uriFileName}");
            if (IsUnAuthorized(response))
            {
                throw new Exception("Unauthorized user please try logging out then logging int");
            }
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("File {FileName} deleted successfully", fileName);
        }

        public async Task<string> GenerateFileLink(string fileName)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",  GetAccessToken());
                var uriFileName = Uri.EscapeUriString(fileName);

                _logger.LogInformation("Generating file link for {FileName} from {_apiBaseUrl}", fileName, _apiBaseUrl);
                var response = await _httpClient.GetAsync(new Uri($"{_apiBaseUrl}/api/Image/image/{uriFileName}"));
                if (IsUnAuthorized(response))
                {
                    throw new Exception("Unauthorized user please try logging out then logging int");
                }
                if (response.IsSuccessStatusCode)
                {
                    var imageUrl = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("File link for {FileName} generated successfully", fileName);
                    return imageUrl;
                }
                _logger.LogWarning("Failed to generate file link for {FileName}", fileName);
                return "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating file link for {FileName}", fileName);
                return "";
            }
        }

        public async Task<PaginatedList<string>> ListFilesAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",  GetAccessToken());

                _logger.LogInformation("Listing files from {_apiBaseUrl} with page {Page} and pageSize {PageSize}", _apiBaseUrl, page, pageSize);
                var response = await _httpClient.GetAsync(new Uri($"{_apiBaseUrl}/api/Image/list?page={page}&pageSize={pageSize}"));
                if (IsUnAuthorized(response))
                {
                    throw new Exception("Unauthorized user please try logging out then logging int");
                }
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var responseBody = await response.Content.ReadAsStringAsync();
                var files = JsonSerializer.Deserialize<PaginatedList<string>>(responseBody, options);

                _logger.LogInformation("Files listed successfully");
                return files;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files");
                return new PaginatedList<string>();
            }
        }

        public async Task<byte[]> GetImageAsync(string fileName)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",  GetAccessToken());

            _logger.LogInformation("Retrieving image {FileName} from {_apiBaseUrl}", fileName, _apiBaseUrl);
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/Image/image/{fileName}");        
            if (IsUnAuthorized(response))
            {
                throw new Exception("Unauthorized user please try logging out then logging int");
            }
            response.EnsureSuccessStatusCode();

            var image = await response.Content.ReadAsByteArrayAsync();
            _logger.LogInformation("Image {FileName} retrieved successfully", fileName);
            return image;
        }

        private string GetAccessToken()
        {
            var accessToken = _httpContextAccessor.HttpContext.Request.Cookies["access_token"];
            if (string.IsNullOrEmpty(accessToken))
            {
                var cookies = _httpContextAccessor.HttpContext.Request.Cookies;
                if (cookies.TryGetValue("BearerToken", out var token))
                {
                    accessToken = token;
                }
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("Unable to retrieve access token");
                throw new InvalidOperationException("Unable to retrieve access token.");
            }

            _logger.LogInformation("Retrieved Access Token: {AccessToken}", accessToken);
            return accessToken;
        }

        private bool IsUnAuthorized(HttpResponseMessage responseMessage) =>
            responseMessage.StatusCode == HttpStatusCode.Unauthorized;
    }
}
