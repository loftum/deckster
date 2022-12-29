using Deckster.Communication;
using Deckster.Core.Domain;
using Deckster.Core.Games;

namespace Deckster.CrazyEights;

[JsonDerived<CrazyEightsCommand>]
public abstract class CrazyEightsCommand : IHaveDiscriminator
{
    public string Discriminator => GetType().Name;
}

public class StartCommand : CrazyEightsCommand
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