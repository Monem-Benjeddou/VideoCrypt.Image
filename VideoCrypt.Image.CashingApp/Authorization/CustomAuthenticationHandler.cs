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
        // Logic for custom authentication can go here.
        // For now, we will simply succeed the authentication.
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "DefaultUser") };
        var identity = new ClaimsIdentity(claims, "DefaultScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "DefaultScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}