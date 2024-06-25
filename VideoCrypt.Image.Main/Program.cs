using System.Net;
using Hydro.Configuration;
using Minio;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHydro();

var minioConfig = builder.Configuration.GetSection("Minio");
var endpoint = minioConfig.GetValue<string>("url");
var accessKey = minioConfig.GetValue<string>("accessKey");
var secretKey = minioConfig.GetValue<string>("secretKey");

var httpClient = new HttpClient(new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

// Register MinIO client as a service
builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(endpoint)
    .WithSSL(false)
    .WithCredentials(accessKey, secretKey));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseHydro(builder.Environment);
app.MapControllers(); // This is important to enable attribute routing for controllers

app.Run();