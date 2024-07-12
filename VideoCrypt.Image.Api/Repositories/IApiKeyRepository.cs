using System.Collections.Generic;
using System.Threading.Tasks;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Repositories
{
    public interface IApiKeyRepository
    {
        Task<IEnumerable<ApiKey>> GetAllApiKeysAsync(string userId);
        Task<ApiKey> GetApiKeyByIdAsync(int id,string userId);
        Task CreateApiKeyAsync(ApiKey apiKey);
        Task DeleteApiKeyAsync(ApiKey apiKey);
        Task<PaginatedList<ApiKey>> GetApiKeysPaginatedAsync(string userId, int pageNumber, int pageSize);
    }
}