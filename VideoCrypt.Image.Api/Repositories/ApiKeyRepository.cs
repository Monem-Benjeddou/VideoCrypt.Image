using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using VideoCrypt.Image.Data.Models;
using VideoCrypt.Image.Data;

namespace VideoCrypt.Image.Api.Repositories
{
    public class ApiKeyRepository(ApplicationDbContext _context) : IApiKeyRepository
    {
        public async Task<IEnumerable<ApiKey>> GetAllApiKeysAsync(string userId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT id, key, name, description, created_at, expire_at FROM api_keys WHERE user_id = @UserId";
            return await connection.QueryAsync<ApiKey>(sql, new { UserId = userId });
        }

        public async Task<ApiKey> GetApiKeyByIdAsync(int id, string  userId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT id, key, name, description, created_at, expire_at FROM api_keys WHERE id = @Id AND user_id = @UserId";
            return await connection.QuerySingleOrDefaultAsync<ApiKey>(sql, new { Id = id, UserId = userId });
        }

        public async Task CreateApiKeyAsync(ApiKey apiKey)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO api_keys (key, name, description, created_at, expire_at, user_id) 
                        VALUES (@Key, @Name, @Description, @CreatedAt, @ExpireAt, @UserId) 
                        RETURNING id";
            var id = await connection.ExecuteScalarAsync<int>(sql, apiKey);
            apiKey.Id = id;
        }

        public async Task DeleteApiKeyAsync(ApiKey apiKey)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM api_keys WHERE id = @Id AND user_id = @UserId";
            await connection.ExecuteAsync(sql, new { Id = apiKey.Id, UserId = apiKey.UserId });
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

            var offset = (pageNumber - 1) * pageSize;

            var apiKeys = await connection.QueryAsync<ApiKey>(sql, new { UserId = userId, Offset = offset, PageSize = pageSize });

            var totalRecords = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(id) FROM api_keys WHERE user_id = @UserId", new { UserId = userId });

            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return new PaginatedList<ApiKey>
            {
                Items = apiKeys.ToList(),
                PageIndex = pageNumber,
                TotalPages = totalPages
            };
        }

    }
}
