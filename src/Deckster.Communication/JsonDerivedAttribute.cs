using System.Text.Json.Serialization;
using Deckster.Core.Games;

namespace Deckster.Communication;

public class JsonDerivedAttribute<T> : JsonConverterAttribute where T : IHaveDiscriminator
{
    public JsonDerivedAttribute() : base(typeof(DerivedTypeConverter<T>))
    {
    }
}