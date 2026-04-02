using System.Text.Json;
using JobHunter.Application.Contracts;

namespace JobHunter.Application.Utilities;

public static class FlexibleIdParser
{
    public static long ParseRequired(JsonElement element, string fieldName)
    {
        if (TryParseId(element, out var id))
        {
            return id;
        }

        throw new InvalidOperationException($"Invalid '{fieldName}' format. Expected number, numeric string, or object with id.");
    }

    public static List<long> ParseList(IEnumerable<JsonElement>? elements, string fieldName)
    {
        if (elements is null)
        {
            return new List<long>();
        }

        var ids = new List<long>();
        var index = 0;
        foreach (var element in elements)
        {
            if (!TryParseId(element, out var id))
            {
                throw new InvalidOperationException($"Invalid '{fieldName}[{index}]' format. Expected number, numeric string, or object with id.");
            }

            ids.Add(id);
            index++;
        }

        return ids;
    }

    public static long ParseRequired(ReqObjectIdDto? dto, string fieldName)
    {
        if (dto is not null)
        {
            return dto.Id;
        }

        throw new InvalidOperationException($"Invalid '{fieldName}' format. Expected number, numeric string, or object with id.");
    }

    public static List<long> ParseList(IEnumerable<ReqObjectIdDto>? dtos, string fieldName)
    {
        if (dtos is null)
        {
            return new List<long>();
        }

        var ids = new List<long>();
        var index = 0;
        foreach (var dto in dtos)
        {
            if (dto is null)
            {
                throw new InvalidOperationException($"Invalid '{fieldName}[{index}]' format. Expected number, numeric string, or object with id.");
            }

            ids.Add(dto.Id);
            index++;
        }

        return ids;
    }

    private static bool TryParseId(JsonElement element, out long id)
    {
        id = 0;
        switch (element.ValueKind)
        {
            case JsonValueKind.Number:
                return element.TryGetInt64(out id);
            case JsonValueKind.String:
                return long.TryParse(element.GetString(), out id);
            case JsonValueKind.Object:
                if (!element.TryGetProperty("id", out var idElement))
                {
                    return false;
                }

                return TryParseId(idElement, out id);
            default:
                return false;
        }
    }
}