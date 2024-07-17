using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using VideoCrypt.Image.Data;
using VideoCrypt.Image.CashingApp.Repository;
using VideoCrypt.Image.Server.Authorization;
using VideoCrypt.Image.Server.Dapper;
using VideoCrypt.Image.Server.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.WithOrigins("dashboard.john-group.org")
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
app.Use(async (context, next) =>
{
    await next.Invoke();

    if (context.Request.Path.StartsWithSegments("/cache"))
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "https://dashboard.john-group.org");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
    }
});
app.UseCors();

app.UseMiddleware<UserIdValidationMiddleware>();

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
