using System.Text.Json.Serialization;
using Deckster.Client.Protocol;

namespace Deckster.Client.Serialization;

public class JsonDerivedAttribute<T>() : JsonConverterAttribute(typeof(DerivedTypeConverter<T>))
    where T : IHaveDiscriminator;