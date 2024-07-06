using Microsoft.AspNetCore.Authorization;

namespace VideoCrypt.Image.Server.Authorization;

public class AccessKeyAuthorization : AuthorizeAttribute
{
    public const string PolicyName = "AccessKeyPolicy";

    public AccessKeyAuthorization()
    {
        Policy = PolicyName;
    }
}