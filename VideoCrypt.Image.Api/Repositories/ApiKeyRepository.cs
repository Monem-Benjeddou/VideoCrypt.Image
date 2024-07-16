using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using VideoCrypt.Image.Data.Models;
using VideoCrypt.Image.Data;

namespace VideoCrypt.Image.Api.Repositories
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApiKeyRepository> _logger;

        public ApiKeyRepository(ApplicationDbContext context, ILogger<ApiKeyRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ApiKey>> GetAllApiKeysAsync(string userId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT id, key, name, description, created_at, expire_at FROM api_keys WHERE user_id = @UserId";

            try
            {
                _logger.LogInformation("Fetching all API keys for user {UserId}", userId);
                return await connection.QueryAsync<ApiKey>(sql, new { UserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching API keys for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ApiKey> GetApiKeyByIdAsync(int id, string userId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT id, key, name, description, created_at, expire_at FROM api_keys WHERE id = @Id AND user_id = @UserId";

            try
            {
                _logger.LogInformation("Fetching API key with ID {Id} for user {UserId}", id, userId);
                return await connection.QuerySingleOrDefaultAsync<ApiKey>(sql, new { Id = id, UserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching API key with ID {Id} for user {UserId}", id, userId);
                throw;
            }
        }

        public async Task CreateApiKeyAsync(ApiKey apiKey)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO api_keys (key, name, description, created_at, expire_at, user_id) 
                        VALUES (@Key, @Name, @Description, @CreatedAt, @ExpireAt, @UserId) 
                        RETURNING id";

            try
            {
                _logger.LogInformation("Creating a new API key for user {UserId}", apiKey.UserId);
                var id = await connection.ExecuteScalarAsync<int>(sql, apiKey);
                apiKey.Id = id;
                _logger.LogInformation("New API key created with ID {Id} for user {UserId}", id, apiKey.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API key for user {UserId}", apiKey.UserId);
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

        public async Task<PaginatedList<ApiKey>> GetApiKeysPaginatedAsync(string userId, int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();
            var sql = @"SELECT id, key, name, description, created_at, expire_at
                FROM api_keys
                WHERE user_id = @UserId
                ORDER BY id
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            try
            {
                _logger.LogInformation("Fetching paginated API keys for user {UserId}, page {PageNumber}, page size {PageSize}", userId, pageNumber, pageSize);

                var offset = (pageNumber - 1) * pageSize;

                var apiKeys = await connection.QueryAsync<ApiKey>(sql, new { UserId = userId, Offset = offset, PageSize = pageSize });

                var totalRecords = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(id) FROM api_keys WHERE user_id = @UserId", new { UserId = userId });

                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                _logger.LogInformation("Paginated API keys fetched successfully for user {UserId}, total pages {TotalPages}", userId, totalPages);

                return new PaginatedList<ApiKey>
                {
                    Items = apiKeys.ToList(),
                    PageIndex = pageNumber,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching paginated API keys for user {UserId}, page {PageNumber}, page size {PageSize}", userId, pageNumber, pageSize);
                throw;
            }
        }
    }
}
