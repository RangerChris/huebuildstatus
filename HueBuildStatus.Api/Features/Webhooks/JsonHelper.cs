using System.Text.Json;

namespace HueBuildStatus.Api.Features.Webhooks;

public static class JsonHelper
{
    public static string? FindJsonProperty(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (GetFromObject(element, propertyName, out var s))
            {
                return s;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            if (GetFromArray(element, propertyName, out var findJsonProperty1))
            {
                return findJsonProperty1;
            }
        }

        return null;
    }

    private static bool GetFromArray(JsonElement element, string propertyName, out string? result)
    {
        result = null;
        foreach (var item in element.EnumerateArray())
        {
            var nestedResult = JsonHelper.FindJsonProperty(item, propertyName);
            if (nestedResult != null)
            {
                result = nestedResult;
                return true;
            }
        }

        return false;
    }

    private static bool GetFromObject(JsonElement element, string propertyName, out string? result)
    {
        result = null;
        foreach (var property in element.EnumerateObject())
        {
            if (property.NameEquals(propertyName))
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    result = property.Value.GetString();
                    return true;
                }
                // else not a string, return null
            }

            var nestedResult = JsonHelper.FindJsonProperty(property.Value, propertyName);
            if (nestedResult != null)
            {
                result = nestedResult;
                return true;
            }
        }

        return false;
    }
}