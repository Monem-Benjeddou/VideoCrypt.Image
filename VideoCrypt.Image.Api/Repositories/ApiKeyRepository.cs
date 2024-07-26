using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using VideoCrypt.Image.Api.Data;
using VideoCrypt.Image.Api.Models;
using VideoCrypt.Image.Data;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Api.Repositories
{
    public class ApiKeyRepository(
        ApplicationDbContext context,
        ILogger<ApiKeyRepository> logger,
        UserManager<IdentityUser> userManager)
        : IApiKeyRepository
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger<ApiKeyRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly UserManager<IdentityUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

        public async Task<IEnumerable<ApiKey>> GetAllApiKeysAsync(ClaimsPrincipal userClaims)
        {
            var user = await _userManager.GetUserAsync(userClaims);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            using var connection = _context.CreateConnection();
            var sql = "SELECT id, key, name, description, created_at, expire_at FROM api_keys WHERE user_id = @UserId";

            try
            {
                _logger.LogInformation("Fetching all API keys for user {UserId}", user.Id);
                return await connection.QueryAsync<ApiKey>(sql, new { UserId = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching API keys for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task<ApiKey> GetApiKeyByIdAsync(int id, ClaimsPrincipal userClaims)
        {
            var user = await _userManager.GetUserAsync(userClaims);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            using var connection = _context.CreateConnection();
            var sql = "SELECT id, key, name, description, created_at, expire_at FROM api_keys WHERE id = @Id AND user_id = @UserId";

            try
            {
                _logger.LogInformation("Fetching API key with ID {Id} for user {UserId}", id, user.Id);
                return await connection.QuerySingleOrDefaultAsync<ApiKey>(sql, new { Id = id, UserId = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching API key with ID {Id} for user {UserId}", id, user.Id);
                throw;
            }
        }

        public async Task<ApiKey> CreateApiKeyAsync(ApiKeyForCreation key, ClaimsPrincipal userClaims)
        {
            var user = await _userManager.GetUserAsync(userClaims);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            try
            {
                var timeNow = DateTime.UtcNow;
                var apiKeyString = ApiKeyGenerator.GenerateApiKey(user.Id,timeNow);
                var apiKey = new ApiKey
                {
                    Key = apiKeyString,
                    Name = key.Name,
                    Description = key.Description,
                    CreatedAt = timeNow,
                    ExpireAt = key.ExpireAt ?? new DateTime(timeNow.Year, timeNow.Month + 1, timeNow.Day),
                    UserId = user.Id
                };
                using var connection = _context.CreateConnection();
                var sql = @"INSERT INTO api_keys (key, name, description, created_at, expire_at, user_id) 
                            VALUES (@Key, @Name, @Description, @CreatedAt, @ExpireAt, @UserId) 
                            RETURNING id";

                _logger.LogInformation("Creating a new API key for user {UserId}", apiKey.UserId);
                var id = await connection.ExecuteScalarAsync<int>(sql, apiKey);
                apiKey.Id = id;
                _logger.LogInformation("New API key created with ID {Id} ", id);
                return apiKey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API key for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task DeleteApiKeyAsync(ApiKey apiKey)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM api_keys WHERE id = @Id AND user_id = @UserId";

            try
            {
                _logger.LogInformation("Deleting API key with ID {Id} for user {UserId}", apiKey.Id, apiKey.UserId);
                await connection.ExecuteAsync(sql, new { Id = apiKey.Id, UserId = apiKey.UserId });
                _logger.LogInformation("API key with ID {Id} deleted successfully for user {UserId}", apiKey.Id, apiKey.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting API key with ID {Id} for user {UserId}", apiKey.Id, apiKey.UserId);
                throw;
            }
        }

        public async Task<PaginatedList<ApiKey>> GetApiKeysPaginatedAsync(ClaimsPrincipal userClaims, int pageNumber, int pageSize)
        {
            var user = await _userManager.GetUserAsync(userClaims);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            using var connection = _context.CreateConnection();
            var sql = @"SELECT id, key, name, description, created_at, expire_at
                        FROM api_keys
                        WHERE user_id = @UserId
                        ORDER BY id
                        OFFSET @Offset
                        LIMIT @Limit";

            try
            {
                _logger.LogInformation("Fetching paginated API keys for user {UserId}", user.Id);
                var apiKeys = await connection.QueryAsync<ApiKey>(sql, new { UserId = user.Id, Offset = (pageNumber - 1) * pageSize, Limit = pageSize });
                var countSql = "SELECT COUNT(*) FROM api_keys WHERE user_id = @UserId";
                var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { UserId = user.Id });
                return new PaginatedList<ApiKey>(){
                    Items = apiKeys.ToList(),
                    TotalPages = totalCount,
                    PageIndex = pageNumber};
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paginated API keys for user {UserId}", user.Id);
                throw;
            }
        }
    }
}
