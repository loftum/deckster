using System.Text.Json.Serialization;
using Deckster.Core.Games;

namespace Deckster.Communication;

public class JsonDerived<T> : JsonConverterAttribute where T : IHaveDiscriminator
{
    public JsonDerived() : base(typeof(DerivedTypeConverter<T>))
    {
    }
}