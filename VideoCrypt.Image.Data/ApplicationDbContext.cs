using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using VideoCrypt.Image.Data.Models;

namespace VideoCrypt.Image.Data;

public class ApplicationDbContext :  IdentityDbContext<IdentityUser>
{
    private readonly string _connectionString;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,IConfiguration configuration)
        : base(options)
    {
        _connectionString = configuration.GetConnectionString("image_db");
        Database.Migrate();
        Database.EnsureCreated();
    }
    public DbSet<ImageMetadata> ImageMetadata { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}