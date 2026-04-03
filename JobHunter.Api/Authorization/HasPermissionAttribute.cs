using Microsoft.AspNetCore.Authorization;
using JobHunter.Application.Utilities;

namespace JobHunter.Api.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string apiPath, string method, string module)
    {
        var permissionValue = PermissionClaimValue.Encode(method, apiPath, module);
        Policy = $"{PermissionPolicyProvider.PermissionPolicyPrefix}{permissionValue}";
    }
}
