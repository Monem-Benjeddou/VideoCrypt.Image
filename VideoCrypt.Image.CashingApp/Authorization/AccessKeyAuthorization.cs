using Microsoft.AspNetCore.Authorization;

namespace VideoCrypt.Image.CashingApp.Authorization;

public abstract class AccessKeyAuthorization : AuthorizeAttribute
{
    public const string PolicyName = "AccessKeyPolicy";

    protected AccessKeyAuthorization()
    {
        Policy = PolicyName;
    }
}