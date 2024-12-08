using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using VideoCrypt.Image.Api.Filters;
using VideoCrypt.Image.Api.Repositories;
using VideoCrypt.Image.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            //http://51.38.80.38
            builder.WithOrigins("https://dashboard.john-group.org/")
                .WithOrigins("https://localhost:8080")
                .WithOrigins("http://localhost:8080")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});
var accessKey = Environment.GetEnvironmentVariable("access_key") ?? throw new Exception("Access key not found");

builder.Services.AddHttpClient("AuthorizedClient",
    client => { client.DefaultRequestHeaders.Add("AccessKey", accessKey); });
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("image_db")));
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(setup =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
    setup.OperationFilter<ExcludeRegisterOperationFilter>();
});

var app = builder.Build();


app.MapPost("/logout",
        async (SignInManager<IdentityUser> signInManager) =>
        {
            await signInManager.SignOutAsync().ConfigureAwait(false);
        })
    .RequireAuthorization();


app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.MapIdentityApi<IdentityUser>();


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();