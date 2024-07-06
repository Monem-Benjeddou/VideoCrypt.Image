using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace VideoCrypt.Image.Server.Authorization;

public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public CustomAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) 
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headers = Request.Headers;
        if (!headers.ContainsKey("AccessKey") || headers["AccessKey"] != "Qqt3KMXNlK4iCKqPhgEd")
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