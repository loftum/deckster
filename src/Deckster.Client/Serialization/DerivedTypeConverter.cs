using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Deckster.Client.Protocol;

namespace Deckster.Client.Serialization;

public class DecksterMessageConverter : JsonConverterFactory
{
    private static readonly Type BaseType = typeof(DecksterMessage);
    private readonly Dictionary<string, Type> _typeMap;

    public DecksterMessageConverter(Dictionary<string, Type> typeMap)
    {
        _typeMap = typeMap;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsSubclassOf(BaseType) && typeToConvert.IsAbstract;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var ctor = typeof(DerivedTypeConverter<>).MakeGenericType(typeToConvert).GetConstructors()[0];
        var instance = ctor.Invoke([_typeMap]);
        return (JsonConverter) instance;
    }
}

public class DerivedTypeConverter<T> : JsonConverter<T> where T : DecksterMessage
{
    // ReSharper disable once StaticMemberInGenericType
    private readonly Dictionary<string, Type> _typeMap;

    public DerivedTypeConverter(Dictionary<string, Type> typeMap)
    {
        _typeMap = typeMap;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var node = JsonNode.Parse(ref reader);
        var discriminator = node?["type"]?.GetValue<string>();
        if (discriminator == null)
        {
            return default;
        }

        if (_typeMap.TryGetValue(discriminator, out var type))
        {
            return (T?) node.Deserialize(type, options);
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize<object>(writer, value, options);
    }
}

public static class TypeExtensions
{
    /// <summary>
    /// Last part
    /// </summary>
    /// <returns>[last part of namespace].[type name], e.g Uno.DrawCardRequest</returns>
    public static string GetGameNamespacedName(this Type type)
    {
        if (type.FullName == null)
        {
            return type.Name;
        }

        var periodCount = 0;
        var start = 0;
        for (var ii = type.FullName.Length - 1; ii >= 0; ii--)
        {
            switch (type.FullName[ii])
            {
                case '.':
                    if (periodCount > 0)
                    {
                        return type.FullName[start..];
                    }
                    periodCount++;
                    break;
            }
            start = ii;
        }
        return type.FullName[start..];
    }
}