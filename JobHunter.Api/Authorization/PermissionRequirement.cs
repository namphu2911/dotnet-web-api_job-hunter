using Microsoft.AspNetCore.Authorization;

namespace JobHunter.Api.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permissionValue)
    {
        PermissionValue = permissionValue;
    }

    public string PermissionValue { get; }
}
