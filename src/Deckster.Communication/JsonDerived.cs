using System.Text.Json.Serialization;

namespace Deckster.Communication;

public class JsonDerived<T> : JsonDerivedTypeAttribute
{
    public JsonDerived() : base(typeof(T), typeof(T).Name)
    {
        
    }
}