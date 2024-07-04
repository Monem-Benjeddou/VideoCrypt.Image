using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Authentication
{
    public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        public override Task SigningIn(CookieSigningInContext context)
        {
            var token = context.Principal?.FindFirst("JWT")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                context.Response.Cookies.Append("BearerToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });
            }

            return base.SigningIn(context);
        }
    }
}