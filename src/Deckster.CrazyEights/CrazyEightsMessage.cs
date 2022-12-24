using Deckster.Communication;
using Deckster.Core.Domain;
using Deckster.CrazyEights.Game;

namespace Deckster.CrazyEights;

[JsonDerived<CrazyEightsMessage>]
[JsonDerived<PlayerPutCardMessage>]
[JsonDerived<PlayerPutEightMessage>]
[JsonDerived<PlayerDrewCardMessage>]
[JsonDerived<PlayerPassedMessage>]
[JsonDerived<ItsYourTurnMessage>]
[JsonDerived<GameStartedMessage>]
public abstract class CrazyEightsMessage
{
    
}

public class PlayerPutCardMessage : CrazyEightsMessage
{
    public Guid PlayerId { get; set; }
    public Card Card { get; set; }
}

public class PlayerPutEightMessage : CrazyEightsMessage
{
    public Guid PlayerId { get; set; }
    public Card Card { get; set; }
    public Suit NewSuit { get; set; }
}

public class PlayerDrewCardMessage : CrazyEightsMessage
{
    public Guid PlayerId { get; set; }
}

public class PlayerPassedMessage : CrazyEightsMessage
{
    public Guid PlayerId { get; set; }
}

public class ItsYourTurnMessage : CrazyEightsMessage
{
    public PlayerViewOfGame PlayerViewOfGame { get; init; }
}

public class GameStartedMessage : CrazyEightsMessage
{
    public PlayerViewOfGame PlayerViewOfGame { get; init; }
}