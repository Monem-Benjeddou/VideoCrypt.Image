using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VideoCrypt.Image.Api.Data;
using VideoCrypt.Image.Api.Models;
using VideoCrypt.Image.Api.Repositories;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApiKeyController(IApiKeyRepository apiKeyRepository, UserManager<IdentityUser> userManager)
        : ControllerBase
    {
        private readonly IApiKeyRepository _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
        private readonly UserManager<IdentityUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApiKey>>> GetApiKeys()
        {
            var user = await _userManager.GetUserAsync(User);
            var apiKeys = await _apiKeyRepository.GetAllApiKeysAsync(user.Id);
            return Ok(apiKeys);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiKey>> GetApiKey(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var apiKey = await _apiKeyRepository.GetApiKeyByIdAsync(id, user.Id);
            if (apiKey == null)
                return NotFound();

            return Ok(apiKey);
        }

        [HttpPost]
        public async Task<ActionResult<ApiKey>> CreateApiKey([FromBody] ApiKeyForCreation key)
        {
            if (key == null)
                return BadRequest("API key data is null.");

            var user = await _userManager.GetUserAsync(User);
            var apiKeyString = ApiKeyGenerator.GenerateApiKey(user.Id);

            var apiKey = new ApiKey
            {
                Key = apiKeyString, // Generate a new unique key with a user signature
                Name = key.Name,
                Description = key.Description,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = key.ExpireAt,
                UserId = user.Id // Associate the API key with the user
            };

            await _apiKeyRepository.CreateApiKeyAsync(apiKey);
            return CreatedAtAction(nameof(GetApiKey), new { id = apiKey.Id }, apiKey);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApiKey(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var apiKey = await _apiKeyRepository.GetApiKeyByIdAsync(id, user.Id);
            if (apiKey == null)
                return NotFound();

            await _apiKeyRepository.DeleteApiKeyAsync(apiKey);
            return NoContent();
        }
    }
}
