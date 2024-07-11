using System.Security.Cryptography;
using System.Text;

namespace VideoCrypt.Image.Api.Data;

public static class ApiKeyGenerator
{ 
    public static string GenerateApiKey(string userId)
    {
        var secretKey = Environment.GetEnvironmentVariable("api_secrete_key");

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userId + DateTime.UtcNow.Ticks));
        return Convert.ToBase64String(hash);
    }
}