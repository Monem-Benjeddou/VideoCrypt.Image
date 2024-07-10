using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Api.Models;
using VideoCrypt.Image.Api.Repositories;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyRepository _apiKeyRepository;

        public ApiKeyController(IApiKeyRepository apiKeyRepository)
        {
            _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApiKey>>> GetApiKeys()
        {
            var apiKeys = await _apiKeyRepository.GetAllApiKeysAsync();
            return Ok(apiKeys);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiKey>> GetApiKey(int id)
        {
            var apiKey = await _apiKeyRepository.GetApiKeyByIdAsync(id);
            if (apiKey == null)
                return NotFound();

            return Ok(apiKey);
        }

        [HttpPost]
        public async Task<ActionResult<ApiKey>> CreateApiKey([FromBody] ApiKeyForCreation key)
        {
            if (key == null)
                return BadRequest("API key data is null.");

            var apiKey = new ApiKey
            {
                Key = Guid.NewGuid().ToString(), // Generate a new unique key
                Name = key.Name,
                Description = key.Description,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = key.ExpireAt
            };

            await _apiKeyRepository.CreateApiKeyAsync(apiKey);
            return CreatedAtAction(nameof(GetApiKey), new { id = apiKey.Id }, apiKey);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApiKey(int id)
        {
            var apiKey = await _apiKeyRepository.GetApiKeyByIdAsync(id);
            if (apiKey == null)
                return NotFound();

            await _apiKeyRepository.DeleteApiKeyAsync(apiKey);
            return NoContent();
        }
    }
}
