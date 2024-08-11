using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deckster.Server.Serialization;

public static class Jsons
{
    public static readonly JsonSerializerOptions CamelCase = new()
    {
        Converters = {new JsonStringEnumConverter()},
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}