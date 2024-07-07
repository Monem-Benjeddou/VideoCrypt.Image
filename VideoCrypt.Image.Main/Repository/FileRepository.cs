using System.Net.Http.Headers;
using System.Text.Json;
using VideoCrypt.Image.Main.Models;

namespace VideoCrypt.Image.Main.Repository
{
    public class FileRepository : IFileRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private string _apiBaseUrl = "http://51.38.80.38:7003";
        private string _apiBaseUrl = "http://51.38.80.38:7003";

        public FileRepository(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
        }

        public async Task UploadFileAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var streamContent = new StreamContent(ms);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(streamContent, "file", file.FileName);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/File/upload", content);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteFileAsync(string fileName)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());

            string uriFileName = Uri.EscapeUriString(fileName);
            
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/File/{uriFileName}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<PaginatedList<string>> ListFilesAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
                var response = await _httpClient.GetAsync(new Uri($"{_apiBaseUrl}/api/file/list?page={page}&pageSize={pageSize}"));
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var responseBody = await response.Content.ReadAsStringAsync();
                var files = JsonSerializer.Deserialize<PaginatedList<string>>(responseBody,options);
                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new PaginatedList<string>();
            }
        }

        public async Task<byte[]> GetImageAsync(string fileName)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());

            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/File/image/{fileName}");
            response.EnsureSuccessStatusCode();

            var image = await response.Content.ReadAsByteArrayAsync();
            return image;
        }

        private async Task<string> GetAccessToken()
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
                throw new InvalidOperationException("Unable to retrieve access token.");
            }

            // Log the token for debugging purposes
            Console.WriteLine($"Retrieved Access Token: {accessToken}");

            return accessToken;
        }

    }
}
