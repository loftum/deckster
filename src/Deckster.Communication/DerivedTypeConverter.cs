using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Deckster.Core.Games;

namespace Deckster.Communication;

public class DerivedTypeConverter<T> : JsonConverter<T> where T : IHaveDiscriminator
{
    private static readonly Dictionary<string, Type> TypeMap;

    static DerivedTypeConverter()
    {
        var types = from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
            where t.IsClass &&
                  !t.IsAbstract &&
                  typeof(T).IsAssignableFrom(t)
            select t;
        TypeMap = types.ToDictionary(t => t.Name, t => t);
    }
        

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var node = JsonNode.Parse(ref reader);
        Console.WriteLine(node.ToJsonString(new JsonSerializerOptions{ WriteIndented = true}));
        var discriminator = node["discriminator"].GetValue<string>();

        if (TypeMap.TryGetValue(discriminator, out var type))
        {
            return (T?) node.Deserialize(type, options);
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize<object>(writer, value, options);
    }

    private struct DiscriminatorHolder : IHaveDiscriminator
    {
        public string Discriminator { get; set; }
    }
}