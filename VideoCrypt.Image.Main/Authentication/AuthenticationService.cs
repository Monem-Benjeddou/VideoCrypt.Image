using System.Text.Json;
using VideoCrypt.Image.Main.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VideoCrypt.Image.Main.Authentication;

public class AuthenticationService
{
    private readonly HttpClient _httpClient;

    public AuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> AuthenticateAsync(string email, string password)
    {
        var loginRequest = new { Email = email, Password = password };
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