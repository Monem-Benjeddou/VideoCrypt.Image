using System;
using System.Text;
using Hydro.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VideoCrypt.Image.Main.Repository;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
string issuer = jwtSettings["Issuer"];
string audience = jwtSettings["Audience"];
string secret = jwtSettings["Secret"];

builder.Services.AddRazorPages();
builder.Services.AddHydro();

builder.Services.AddHttpClient();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    });

builder.Services.AddScoped<IFileRepository, FileRepository>();

builder.Logging.AddConsole();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure to redirect to login if not signed in
app.Use(async (context, next) =>
{
    var user = context.User;

    if (user?.Identity?.IsAuthenticated != true)
    {
        // Redirect to login page
        context.Response.Redirect("/Login");
        return;
    }

    await next();
});

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseHydro(app.Environment);
app.MapControllers(); 

app.Run();