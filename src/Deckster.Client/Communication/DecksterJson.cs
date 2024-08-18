using System.Text.Json;
using System.Text.Json.Serialization;
using Deckster.Client.Common;

namespace Deckster.Client.Communication;

public static class DecksterJson
{
    public static readonly JsonSerializerOptions Options = Create();
    
    public static readonly JsonSerializerOptions PrettyOptions = Create(o => o.WriteIndented = true);

    private static JsonSerializerOptions Create(Action<JsonSerializerOptions>? configure = null)
    {
      var options = new JsonSerializerOptions
      {
          Converters = {new JsonStringEnumConverter(),
              new DerivedTypeConverter<DecksterCommand>(),
              new DerivedTypeConverter<CommandResult>()},
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
          AllowTrailingCommas = true,
          ReadCommentHandling = JsonCommentHandling.Skip,
      };
      configure?.Invoke(options);
      return options;
    }
    
    public static T? Deserialize<T>(ReadOnlySpan<byte> bytes)
    {
        return JsonSerializer.Deserialize<T>(bytes, Options);
    }

    public static byte[] SerializeToBytes(object item)
    {
        return JsonSerializer.SerializeToUtf8Bytes(item, item.GetType(), Options);
    }

    public static string Pretty(this object? item)
    {
        return item == null ? "null" : JsonSerializer.Serialize(item, item.GetType(), PrettyOptions);
    }
}