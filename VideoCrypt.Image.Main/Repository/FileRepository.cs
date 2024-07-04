using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace VideoCrypt.Image.Main.Repository
{
    public class FileRepository : IFileRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _apiBaseUrl = "http://localhost:7001";

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

        public async Task<List<byte[]>> ListFilesAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());

            var response = await _httpClient.GetFromJsonAsync<List<byte[]>>(new Uri($"{_apiBaseUrl}/api/File/list"));
            return response ?? new List<byte[]>();
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
