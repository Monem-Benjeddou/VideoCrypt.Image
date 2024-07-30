using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using VideoCrypt.Image.Dashboard.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VideoCrypt.Image.Dashboard.Authentication;

public class AuthenticationService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<string> AuthenticateAsync(string email, string password)
    {
        var loginRequest = new { Email = email, Password = password };
        Console.WriteLine(_httpClient.BaseAddress);
        var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}login", loginRequest);
        
        if (!response.IsSuccessStatusCode) return null;
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse,serializeOptions);
        return tokenResponse.AccessToken;

    }
}