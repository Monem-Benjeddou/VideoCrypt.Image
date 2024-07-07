using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using VideoCrypt.Image.Data;
using VideoCrypt.Image.CashingApp.Repository;
using VideoCrypt.Image.Server.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuthentication("DefaultScheme")
    .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>("DefaultScheme", null);
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(8080); 
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AccessKeyAuthorization.PolicyName, policy =>
        policy.Requirements.Add(new AccessKeyRequirement("Qqt3KMXNlK4iCKqPhgEd")));
});

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