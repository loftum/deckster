using Deckster.Communication;
using Deckster.Core.Domain;
using Deckster.Core.Games;
using Deckster.CrazyEights.Game;

namespace Deckster.CrazyEights;

[JsonDerived<CrazyEightsMessage>]
public abstract class CrazyEightsMessage : IHaveDiscriminator
{
    public string Discriminator => GetType().Name;
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

public class GameEndedMessage : CrazyEightsMessage
{
    public PlayerViewOfGame PlayerViewOfGame { get; init; }
}