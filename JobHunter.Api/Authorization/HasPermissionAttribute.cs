using Microsoft.AspNetCore.Authorization;

namespace JobHunter.Api.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = $"{PermissionPolicyProvider.PermissionPolicyPrefix}{permission}";
    }
}
