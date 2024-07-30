using System.Threading.Tasks;
using VideoCrypt.Image.Dashboard.Models;

namespace VideoCrypt.Image.Dashboard.Repositories;

public interface IApiKeyRepository
{
    Task<ApiKey> GetApiKeyAsync(int id);
    Task<PaginatedList<ApiKey>> GetApiKeysAsync(int page, int size);
    Task<ApiKey> CreateApiKeyAsync(ApiKeyForCreation apiKeyForCreation);
    Task<bool> DeleteApiKeyAsync(int id);
}