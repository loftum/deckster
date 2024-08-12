using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deckster.Client.Serialization;

public static class Jsons
{
    public static readonly JsonSerializerOptions CamelCase = new()
    {
        Converters = {new JsonStringEnumConverter()},
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static readonly JsonSerializerOptions PrettyOptions = new()
    {
        Converters = {new JsonStringEnumConverter()},
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Pretty(object item)
    {
        return JsonSerializer.Serialize(item, item.GetType(), PrettyOptions);
    }
}