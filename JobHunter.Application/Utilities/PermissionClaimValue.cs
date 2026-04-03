namespace JobHunter.Application.Utilities;

public static class PermissionClaimValue
{
    private const char Delimiter = '|';

    public static string Encode(string method, string apiPath, string module)
    {
        var normalizedMethod = (method ?? string.Empty).Trim().ToUpperInvariant();
        var normalizedPath = (apiPath ?? string.Empty).Trim();
        var normalizedModule = (module ?? string.Empty).Trim().ToUpperInvariant();

        return string.Join(Delimiter, normalizedMethod, normalizedPath, normalizedModule);
    }
}