using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace VideoCrypt.Image.CashingApp.Authorization
{
    public class CustomAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var headers = Request.Headers;
            var path = Request.Path.Value;
            var accessKey = Environment.GetEnvironmentVariable("access_key")?? throw new Exception("Access key not found");
            if ((!headers.ContainsKey("AccessKey") || headers["AccessKey"] != accessKey) && !path.Contains("/cache/") )
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Access Key"));
            }

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "DefaultUser") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}