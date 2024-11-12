using System.Text.Json;
using System.Text.Json.Serialization;
using Deckster.Core.Protocol;

namespace Deckster.Core.Serialization;

public class DerivedTypeConverter<T> : JsonConverter<T> where T : DecksterMessage
{
    private readonly Dictionary<string, Type> _typeMap;

    public DerivedTypeConverter(Dictionary<string, Type> typeMap)
    {
        _typeMap = typeMap;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var element = JsonElement.ParseValue(ref reader);
        if (!element.TryGetProperty("type", out var p) || p.ValueKind != JsonValueKind.String)
        {
            return default;
        }

        var discriminator = p.GetString();
        if (discriminator == null)
        {
            return default;
        }

        if (_typeMap.TryGetValue(discriminator, out var type))
        {
            return (T?) element.Deserialize(type, options);
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
        if (type.IsNullable(out var inner))
        {
            type = inner;
        }
        
        var fullName = string.Join('.',type.Namespace, type.ToOpenApiFriendlyName());

        var periodCount = 0;
        var start = 0;
        for (var ii = fullName.Length - 1; ii >= 0; ii--)
        {
            switch (fullName[ii])
            {
                case '.':
                    if (periodCount > 0)
                    {
                        return fullName[start..];
                    }
                    periodCount++;
                    break;
            }
            start = ii;
        }
        return fullName[start..];
    }

    public static bool IsNullable(this Type type, out Type inner)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            inner = type.GenericTypeArguments[0];
            return true;
        }

        inner = default;
        return false;
    }

    private static readonly Dictionary<Type, string> Simple = new()
    {
        [typeof(int)] = "int",
        [typeof(bool)] = "bool",
        [typeof(double)] = "double",
        [typeof(string)] = "string",
    };

    public static string ToOpenApiFriendlyName(this Type type)
    {
        if (!type.IsGenericType)
        {
            if (Simple.TryGetValue(type, out var friendly))
            {
                return friendly;
            }
            if (type.IsNullable(out var inner))
            {
                return Simple.TryGetValue(inner, out friendly)
                    ? $"{friendly}?"
                    : $"{type.Name}?";
            }
            
            return type.Name;
        }

        var arguments = type.GetGenericArguments();
        
        var index = type.Name.IndexOf('`');
        var b = type.Name[..index];
        return $"{b}Of{string.Join(',', arguments.Select(ToDisplayString))}";
    }

    public static string ToDisplayString(this Type type)
    {
        if (!type.IsGenericType)
        {
            if (Simple.TryGetValue(type, out var friendly))
            {
                return friendly;
            }
            if (type.IsNullable(out var inner))
            {
                return Simple.TryGetValue(inner, out friendly)
                    ? $"{friendly}?"
                    : $"{type.Name}?";
            }
            
            return type.Name;
        }

        var arguments = type.GetGenericArguments();
        
        var index = type.Name.IndexOf('`');
        var b = type.Name[..index];
        return $"{b}<{string.Join(',', arguments.Select(ToDisplayString))}>";
    }
}