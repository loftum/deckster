using Deckster.Client.Games.Common;
using Deckster.Client.Protocol;

namespace Deckster.Client.Games.CrazyEights;

public abstract class CrazyEightsRequest : IHaveDiscriminator
{
    public string Type { get; }
}

public class PutCardRequest : CrazyEightsRequest
{
    public Card Card { get; set; }
}

public class PutEightRequest : CrazyEightsRequest
{
    public Card Card { get; set; }
    public Suit NewSuit { get; set; }
}

public class DrawCardRequest : CrazyEightsRequest
{
    
}

public class PassRequest : CrazyEightsRequest
{
    
}

public abstract class CrazyEightsResponse : IHaveDiscriminator
{
    public string Type { get; }
}
