using Deckster.Communication;
using Deckster.Core.Domain;

namespace Deckster.CrazyEights;

[JsonDerived<CrazyEightsMessage>]
[JsonDerived<PlayerPutCardMessage>]
[JsonDerived<PlayerPutEightMessage>]
[JsonDerived<PlayerDrewCardMessage>]
[JsonDerived<PlayerPassedMessage>]
[JsonDerived<ItsYourTurnMessage>]
public abstract class CrazyEightsMessage
{
    public Guid PlayerId { get; set; }
}

public class PlayerPutCardMessage : CrazyEightsMessage
{
    public Card Card { get; set; }
}

public class PlayerPutEightMessage : CrazyEightsMessage
{
    public Card Card { get; set; }
    public Suit NewSuit { get; set; }
}

public class PlayerDrewCardMessage : CrazyEightsMessage
{
    
}

public class PlayerPassedMessage : CrazyEightsMessage
{
    
}

public class ItsYourTurnMessage : CrazyEightsMessage
{
    
}