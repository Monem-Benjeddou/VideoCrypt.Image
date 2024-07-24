using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using VideoCrypt.Image.Api.Models;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Repositories
{
    public interface IApiKeyRepository
    {
        Task<IEnumerable<ApiKey>> GetAllApiKeysAsync(ClaimsPrincipal userClaims);
        Task<ApiKey> GetApiKeyByIdAsync(int id, ClaimsPrincipal userClaims);
        Task<ApiKey> CreateApiKeyAsync(ApiKeyForCreation key, ClaimsPrincipal userClaims);
        Task DeleteApiKeyAsync(ApiKey apiKey);
        Task<PaginatedList<ApiKey>> GetApiKeysPaginatedAsync(ClaimsPrincipal userClaims, int pageNumber, int pageSize);
    }
}