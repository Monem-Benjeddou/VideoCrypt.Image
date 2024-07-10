using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Repositories;

public interface IApiKeyRepository
{
    Task<IEnumerable<ApiKey>> GetAllApiKeysAsync();
    Task<ApiKey> GetApiKeyByIdAsync(int id);
    Task CreateApiKeyAsync(ApiKey apiKey);
    Task DeleteApiKeyAsync(ApiKey apiKey);
}