using VideoCrypt.Image.Main.Models;

namespace VideoCrypt.Image.Main.Repository;

public interface IApiKeyRepository
{
    Task<ApiKey> GetApiKeyAsync(int id);
    Task<PaginatedList<ApiKey>> GetApiKeysAsync(int page, int size);
    Task<ApiKey> CreateApiKeyAsync(ApiKeyForCreation apiKeyForCreation);
    Task<bool> DeleteApiKeyAsync(int id);
}