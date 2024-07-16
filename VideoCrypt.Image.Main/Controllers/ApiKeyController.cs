using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Main.Models;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiKeysController(IApiKeyRepository apiKeyRepository) : ControllerBase
{
    [HttpPost("CreateApiKey")]
    public async Task<IActionResult> CreateApiKey(ApiKeyForCreation apiKeyForCreation)
    {
        var apiKey = await apiKeyRepository.CreateApiKeyAsync(apiKeyForCreation);
        return RedirectToPage("apikey");
    }
}
