using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        private readonly string _connectionString;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _connectionString = configuration.GetConnectionString("image_db");
        }

        public DbSet<ImageMetadata> ImageMetadata { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                if (entity.ClrType == typeof(ImageMetadata) || entity.ClrType == typeof(ApiKey))
                {
                    entity.SetTableName(entity.GetTableName().ToSnakeCase());

                    foreach (var property in entity.GetProperties())
                    {
                        property.SetColumnName(property.GetColumnName().ToSnakeCase());
                    }

                    foreach (var key in entity.GetKeys())
                    {
                        key.SetName(key.GetName().ToSnakeCase());
                    }

                    foreach (var key in entity.GetForeignKeys())
                    {
                        key.SetConstraintName(key.GetConstraintName().ToSnakeCase());
                    }

                    foreach (var index in entity.GetIndexes())
                    {
                        index.SetDatabaseName(index.GetDatabaseName().ToSnakeCase());
                    }
                }
            }
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }

    public static class StringExtensions
    {
        public static string ToSnakeCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var snakeCase = string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));
            return snakeCase.ToLower();
        }
    }
}
