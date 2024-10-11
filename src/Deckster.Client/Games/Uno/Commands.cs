using Deckster.Client.Protocol;
using Deckster.Client.Serialization;

namespace Deckster.Client.Games.Uno;

[JsonDerived<UnoRequest>]
public abstract class UnoRequest : IHaveDiscriminator
{
    public string Type { get; }
}

public class PutCardRequest : UnoRequest
{
    public UnoCard Card { get; set; }
}

public class PutWildRequest : UnoRequest
{
    public UnoCard Card { get; set; }
    public UnoColor NewColor { get; set; }
}

public class ReadyToPlayRequest : UnoRequest
{
    
}

public class DrawCardRequest : UnoRequest
{
    
}

public class PassRequest : UnoRequest
{
    
}