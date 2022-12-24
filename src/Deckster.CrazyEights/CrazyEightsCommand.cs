using Deckster.Communication;
using Deckster.Core.Domain;

namespace Deckster.CrazyEights;

[JsonDerived<PutCardCommand>]
[JsonDerived<PutEightCommand>]
[JsonDerived<DrawCardCommand>]
[JsonDerived<PassCommand>]
public abstract class CrazyEightsCommand
{
    
}

public class PutCardCommand : CrazyEightsCommand
{
    public Card Card { get; set; }
}

public class PutEightCommand : CrazyEightsCommand
{
    public Card Card { get; set; }
    public Suit NewSuit { get; set; }
}

public class DrawCardCommand : CrazyEightsCommand
{
    
}

public class PassCommand : CrazyEightsCommand
{
    
}