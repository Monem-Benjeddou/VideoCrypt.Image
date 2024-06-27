using System.Net;
using System.Net.Http;
using Hydro.Configuration;
using Microsoft.AspNetCore.Builder;
using Minio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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


var app = builder.Build();

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