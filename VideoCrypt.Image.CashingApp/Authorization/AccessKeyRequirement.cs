using Microsoft.AspNetCore.Authorization;

namespace VideoCrypt.Image.CashingApp.Authorization;

public class AccessKeyRequirement(string accessKey) : IAuthorizationRequirement
{
    public string AccessKey { get; } = accessKey;
}