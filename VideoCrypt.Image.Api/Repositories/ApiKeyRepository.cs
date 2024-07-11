using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using VideoCrypt.Image.Data.Models;
using VideoCrypt.Image.Data;

namespace VideoCrypt.Image.Api.Repositories
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly ApplicationDbContext _context;

        public ApiKeyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApiKey>> GetAllApiKeysAsync(string userId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT id, key, name, description, created_at, expire_at FROM api_keys WHERE user_id = @UserId";
            return await connection.QueryAsync<ApiKey>(sql, new { UserId = userId });
        }

        public async Task<ApiKey> GetApiKeyByIdAsync(int id, string userId)
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
    }
}
