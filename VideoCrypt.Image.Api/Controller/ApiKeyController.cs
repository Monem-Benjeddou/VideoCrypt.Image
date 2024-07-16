using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoCrypt.Image.Api.Data;
using VideoCrypt.Image.Api.Models;
using VideoCrypt.Image.Api.Repositories;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApiKeyController(
        IApiKeyRepository apiKeyRepository,
        UserManager<IdentityUser> userManager,
        ILogger<ApiKeyController> logger)
        : ControllerBase
    {
        private readonly IApiKeyRepository _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
        private readonly UserManager<IdentityUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly ILogger<ApiKeyController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ApiKey>>> GetAllApiKeys()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var apiKeys = await _apiKeyRepository.GetAllApiKeysAsync(user.Id);
                return Ok(apiKeys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting API keys.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiKey>> GetApiKey(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var apiKey = await _apiKeyRepository.GetApiKeyByIdAsync(id, user.Id);
                if (apiKey == null)
                {
                    _logger.LogWarning("API key with id {Id} not found for user {UserId}", id, user.Id);
                    return NotFound();
                }

                return Ok(apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the API key with id {Id}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiKey>> CreateApiKey([FromBody] ApiKeyForCreation key)
        {
            if (key == null)
            {
                _logger.LogWarning("API key creation request is null.");
                return BadRequest("API key data is null.");
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                var apiKeyString = ApiKeyGenerator.GenerateApiKey(user.Id);

                var apiKey = new ApiKey
                {
                    Key = apiKeyString,
                    Name = key.Name,
                    Description = key.Description,
                    CreatedAt = DateTime.UtcNow,
                    ExpireAt = key.ExpireAt,
                    UserId = user.Id
                };

                await _apiKeyRepository.CreateApiKeyAsync(apiKey);
                return CreatedAtAction(nameof(GetApiKey), new { id = apiKey.Id }, apiKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an API key.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApiKey(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var apiKey = await _apiKeyRepository.GetApiKeyByIdAsync(id, user.Id);
                if (apiKey == null)
                {
                    _logger.LogWarning("API key with id {Id} not found for user {UserId}", id, user.Id);
                    return NotFound();
                }
                
                await _apiKeyRepository.DeleteApiKeyAsync(apiKey);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the API key with id {Id}.", id);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ApiKey>>> GetApiKeys([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var apiKeys = await _apiKeyRepository.GetApiKeysPaginatedAsync(user.Id, pageNumber, pageSize);
                return Ok(apiKeys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting API keys.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
