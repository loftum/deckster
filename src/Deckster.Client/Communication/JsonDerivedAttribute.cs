using System.Text.Json.Serialization;
using Deckster.Client.Core.Games;

namespace Deckster.Client.Communication;

public class JsonDerivedAttribute<T> : JsonConverterAttribute where T : IHaveDiscriminator
{
    public JsonDerivedAttribute() : base(typeof(DerivedTypeConverter<T>))
    {
    }
}