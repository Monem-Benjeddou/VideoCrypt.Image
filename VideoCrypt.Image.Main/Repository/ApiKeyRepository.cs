using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using VideoCrypt.Image.Main.Models;

namespace VideoCrypt.Image.Main.Repository
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiKeyRepository> _logger;
        private string _apiBaseUrl = "https://api.john-group.org";

        public ApiKeyRepository(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<ApiKeyRepository> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
        }

        public async Task<ApiKey> GetApiKeyAsync(int id)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());

            _logger.LogInformation("Getting API key with id {Id} from {_apiBaseUrl}", id, _apiBaseUrl);
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/ApiKey/{id}");
            if (IsUnAuthorized(response))
            {
                throw new Exception("Unauthorized user. Please try logging out then logging in.");
            }
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var apiKey = JsonSerializer.Deserialize<ApiKey>(responseBody);

            _logger.LogInformation("API key with id {Id} retrieved successfully", id);
            return apiKey;
        }

        public async Task<PaginatedList<ApiKey>> GetApiKeysAsync(int page = 1, int pageSize = 10)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());

            _logger.LogInformation("Getting all API keys from {_apiBaseUrl}", _apiBaseUrl);
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/ApiKey?page={page}&pageSize={pageSize}");

            if (IsUnAuthorized(response))
            {
                throw new Exception("Unauthorized user. Please try logging out then logging in.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to retrieve API keys. Status code: {StatusCode}, Response: {Response}", response.StatusCode, errorResponse);
                response.EnsureSuccessStatusCode(); // This will still throw an exception with detailed info.
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var apiKeys = JsonSerializer.Deserialize<PaginatedList<ApiKey>>(responseBody);

            if (apiKeys == null)
            {
                _logger.LogError("Failed to deserialize API keys. Response: {Response}", responseBody);
                throw new Exception("Failed to retrieve API keys. Please try again later.");
            }

            _logger.LogInformation("All API keys retrieved successfully");
            return apiKeys;
        }


        public async Task<ApiKey> CreateApiKeyAsync(ApiKeyForCreation key)
        {
            if (key == null)
            {
                _logger.LogWarning("API key creation request is null.");
                throw new ArgumentNullException(nameof(key));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());

            var jsonContent = new StringContent(JsonSerializer.Serialize(key), System.Text.Encoding.UTF8, "application/json");

            _logger.LogInformation("Creating new API key at {_apiBaseUrl}", _apiBaseUrl);
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/api/ApiKey", jsonContent);
            if (IsUnAuthorized(response))
            {
                throw new Exception("Unauthorized user. Please try logging out then logging in.");
            }
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var createdApiKey = JsonSerializer.Deserialize<ApiKey>(responseBody);

            _logger.LogInformation("API key created successfully");
            return createdApiKey;
        }

        public async Task<bool> DeleteApiKeyAsync(int id)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());

            _logger.LogInformation("Deleting API key with id {Id} from {_apiBaseUrl}", id, _apiBaseUrl);
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/api/ApiKey/{id}");
            if (IsUnAuthorized(response))
            {
                throw new Exception("Unauthorized user. Please try logging out then logging in.");
            }
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("API key with id {Id} deleted successfully", id);
            return response.IsSuccessStatusCode;
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
