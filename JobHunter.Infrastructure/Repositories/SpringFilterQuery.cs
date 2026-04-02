using System.Text.RegularExpressions;

namespace JobHunter.Infrastructure.Repositories;

internal static class SpringFilterQuery
{
    public static string Normalize(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return string.Empty;
        }

        var decoded = Uri.UnescapeDataString(filter).Replace('+', ' ');
        return decoded.Trim();
    }

    public static IReadOnlyList<string> GetContainsValues(string? filter, string field)
    {
        var input = Normalize(filter);
        if (input.Length == 0)
        {
            return Array.Empty<string>();
        }

        var pattern = $@"\b{Regex.Escape(field)}\s*~\s*'([^']*)'";
        return Regex.Matches(input, pattern, RegexOptions.IgnoreCase)
            .Select(m => m.Groups[1].Value.Trim())
            .Where(v => v.Length > 0)
            .ToArray();
    }

    public static IReadOnlyList<string> GetEqualsValues(string? filter, string field)
    {
        var input = Normalize(filter);
        if (input.Length == 0)
        {
            return Array.Empty<string>();
        }

        var pattern = $@"\b{Regex.Escape(field)}\s*(?:=|==)\s*'([^']*)'";
        return Regex.Matches(input, pattern, RegexOptions.IgnoreCase)
            .Select(m => m.Groups[1].Value.Trim())
            .Where(v => v.Length > 0)
            .ToArray();
    }

    public static IReadOnlyList<string> GetInValues(string? filter, string field)
    {
        var input = Normalize(filter);
        if (input.Length == 0)
        {
            return Array.Empty<string>();
        }

        var pattern = $@"\b{Regex.Escape(field)}\s+in\s*\(([^)]*)\)";
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return Array.Empty<string>();
        }

        var raw = match.Groups[1].Value;
        var values = Regex.Matches(raw, @"'([^']*)'|([^,\s]+)")
            .Select(m => m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value)
            .Select(v => v.Trim())
            .Where(v => v.Length > 0)
            .ToArray();

        return values;
    }

    public static bool TryParseSort(string? sort, out string field, out bool desc)
    {
        field = string.Empty;
        desc = false;

        if (string.IsNullOrWhiteSpace(sort))
        {
            return false;
        }

        var decoded = Uri.UnescapeDataString(sort).Trim();
        var parts = decoded.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        field = parts[0];
        var direction = parts[1];
        desc = direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return direction.Equals("asc", StringComparison.OrdinalIgnoreCase) || desc;
    }
}