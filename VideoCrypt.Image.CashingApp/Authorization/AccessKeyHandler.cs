using Microsoft.AspNetCore.Authorization;
namespace VideoCrypt.Image.Server.Authorization;

public class AccessKeyHandler : AuthorizationHandler<AccessKeyRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccessKeyHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessKeyRequirement requirement)
    {
        var request = _httpContextAccessor.HttpContext.Request;

        if (request.Headers.TryGetValue("AccessKey", out var extractedKey))
        {
            if (extractedKey == requirement.AccessKey)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
