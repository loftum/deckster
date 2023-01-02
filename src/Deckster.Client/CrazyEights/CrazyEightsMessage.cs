using Deckster.Client.Communication;
using Deckster.Client.Communication.Handshake;
using Deckster.Client.Core.Domain;
using Deckster.Client.Core.Games;

namespace Deckster.Client.CrazyEights;

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
    public List<PlayerData> Players { get; init; }
}