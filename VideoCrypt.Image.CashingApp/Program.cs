using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using VideoCrypt.Image.Data;
using VideoCrypt.Image.CashingApp.Repository;
using VideoCrypt.Image.Data.Models;
using VideoCrypt.Image.Server.Authorization;
using VideoCrypt.Image.Server.Dapper;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.WithOrigins("http://51.38.80.38:8080")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddAuthentication("DefaultScheme")
    .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>("DefaultScheme", null);
var accessKey = Environment.GetEnvironmentVariable("access_key") ?? throw new Exception("Access key not found");
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AccessKeyAuthorization.PolicyName, policy =>
        policy.Requirements.Add(new AccessKeyRequirement(accessKey)));
});

DapperExtensions.ConfigureTypeMappings();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("image_db")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var cachePath = Path.Combine(app.Environment.ContentRootPath, "cache");
if (!Directory.Exists(cachePath))
{
    Directory.CreateDirectory(cachePath);
}
app.UseCors(); 

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(cachePath),
    RequestPath = "/cache"
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();