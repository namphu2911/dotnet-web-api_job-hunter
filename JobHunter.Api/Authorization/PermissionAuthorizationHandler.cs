using Microsoft.AspNetCore.Authorization;

namespace JobHunter.Api.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var hasWildcard = context.User.Claims.Any(c => c.Type == "permission" && c.Value == "*");
        var hasPermission = context.User.Claims.Any(c =>
            c.Type == "permission" &&
            string.Equals(c.Value, requirement.PermissionValue, StringComparison.Ordinal));

        if (hasWildcard || hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
