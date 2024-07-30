using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace VideoCrypt.Image.Dashboard.Authentication
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