using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using VideoCrypt.Image.Data.Models;
using Npgsql;
using VideoCrypt.Image.Data;

namespace VideoCrypt.Image.Api.Repositories;
public class ApiKeyRepository(ApplicationDbContext _context) : IApiKeyRepository
{
    
    public async Task<IEnumerable<ApiKey>> GetAllApiKeysAsync()
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT id, key, name, description, created_at, expire_at FROM api_keys";
        return await connection.QueryAsync<ApiKey>(sql);
    }

    public async Task<ApiKey> GetApiKeyByIdAsync(int id)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT id, key, name, description, created_at, expire_at FROM api_keys WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<ApiKey>(sql, new { Id = id });
    }

    public async Task CreateApiKeyAsync(ApiKey apiKey)
    {
        using var connection = _context.CreateConnection();
        var sql = @"INSERT INTO api_keys (key, name, description, created_at, expire_at) 
                    VALUES (@Key, @Name, @Description, @CreatedAt, @ExpireAt) 
                    RETURNING id";
        var id = await connection.ExecuteScalarAsync<int>(sql, apiKey);
        apiKey.Id = id;
    }

    public async Task DeleteApiKeyAsync(ApiKey apiKey)
    {
        using var connection = _context.CreateConnection();
        var sql = "DELETE FROM api_keys WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = apiKey.Id });
    }
}