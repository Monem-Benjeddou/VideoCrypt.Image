using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using VideoCrypt.Image.Main.Models;

namespace VideoCrypt.Image.Main.Repository;

public class UserRepository(IHttpClientFactory httpClientFactory) : IUserRepository
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    public async Task<string> AuthenticateAsync(string email, string password)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var loginRequest = new { Email = email, Password = password };

        var jsonRequest = JsonSerializer.Serialize(loginRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("login", content);
        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return tokenResponse.AccessToken;
        }

        return null;
    }

    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var refreshRequest = new { RefreshToken = refreshToken };

        var jsonRequest = JsonSerializer.Serialize(refreshRequest);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("refresh", content);
        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return tokenResponse.AccessToken;
        }

        return null;
    }
}