using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VideoCrypt.Image.Api.Models;
using VideoCrypt.Image.Api.Repositories;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApiKeyController(
        IApiKeyRepository _apiKeyRepository,
        ILogger<ApiKeyController> _logger)
        : ControllerBase
    {

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ApiKey>>> GetAllApiKeys()
        {
            try
            {
                var apiKeys = await _apiKeyRepository.GetAllApiKeysAsync(User);
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
                var apiKey = await _apiKeyRepository.GetApiKeyByIdAsync(id, User);
                if (apiKey != null) return Ok(apiKey);
                _logger.LogWarning("API key with id {Id} not found for user.", id);
                return NotFound();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the API key with id {Id}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiKey>> CreateApiKey([FromBody] ApiKeyForCreation? key)
        {
            if (key == null)
            {
                _logger.LogWarning("API key creation request is null.");
                return BadRequest("API key data is null.");
            }
            try
            {
                var apiKey = await _apiKeyRepository.CreateApiKeyAsync(key, User);
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
                var apiKey = await _apiKeyRepository.GetApiKeyByIdAsync(id, User);
                if (apiKey == null)
                {
                    _logger.LogWarning("API key with id {Id} not found for user.", id);
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
                var apiKeys = await _apiKeyRepository.GetApiKeysPaginatedAsync(User, pageNumber, pageSize);
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
