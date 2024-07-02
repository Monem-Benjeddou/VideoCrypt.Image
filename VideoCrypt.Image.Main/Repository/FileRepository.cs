using System.Net.Http.Headers;

namespace VideoCrypt.Image.Main.Repository
{
    public class FileRepository : IFileRepository
    {
        private readonly HttpClient _httpClient;
        private string _apiBaseUrl = "http://localhost:5147";
        public FileRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

            var response = await _httpClient.PostAsync("${_apiBaseUrl}/api/File/upload", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<byte[]>> ListFilesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<byte[]>>("${_apiBaseUrl}/api/File/list");
            return response ?? [];
        }

        public async Task<byte[]> GetImageAsync(string fileName)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/File/image/{fileName}");
            response.EnsureSuccessStatusCode();

            var image = await response.Content.ReadAsByteArrayAsync();
            return image;
        }
    }
}