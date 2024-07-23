using Microsoft.AspNetCore.Authorization;

namespace VideoCrypt.Image.CashingApp.Authorization;

public class AccessKeyHandler(IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<AccessKeyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessKeyRequirement requirement)
    {
        var request = httpContextAccessor.HttpContext.Request;

        if (!request.Headers.TryGetValue("AccessKey", out var extractedKey)) return Task.CompletedTask;
        if (extractedKey == requirement.AccessKey)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
