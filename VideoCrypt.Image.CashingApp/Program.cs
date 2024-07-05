using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using VideoCrypt.Image.Data;
using VideoCrypt.Image.CashingApp.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
var env = app.Environment;
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "cache")),
    RequestPath = "/cache"
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();