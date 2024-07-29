using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace VideoCrypt.Image.Main.Middlewares;

public class JwtMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task Invoke(HttpContext context)  
    {  
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();  

        if (token != null)  
            attachAccountToContext(context, token);  

        await next(context);  
    }  

    private void attachAccountToContext(HttpContext context, string token)  
    {  
        try  
        {  
            var tokenHandler = new JwtSecurityTokenHandler();  
            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);  
            tokenHandler.ValidateToken(token, new TokenValidationParameters  
            {  
                ValidateIssuerSigningKey = true,  
                IssuerSigningKey = new SymmetricSecurityKey(key),  
                ValidateIssuer = false,  
                ValidateAudience = false,  
                ClockSkew = TimeSpan.Zero  
            }, out SecurityToken validatedToken);  
              
            var jwtToken = (JwtSecurityToken)validatedToken;  
              
            var username = jwtToken.Claims.First(x => x.Type == "username").Value;  
            var role = jwtToken.Claims.First(x => x.Type == "role").Value;  


            var userClaims = new List<Claim>()  
            {  
                new Claim("UserName", username),   
                new Claim("Role", role)  
             };  

            var userIdentity = new ClaimsIdentity(userClaims, "User Identity");  

            var userPrincipal = new ClaimsPrincipal(new[] { userIdentity });  

            context.SignInAsync(userPrincipal);  
        }  
        catch (Exception ex)  
        {  
            throw new Exception(ex.Message);  
        }  
    }  
}