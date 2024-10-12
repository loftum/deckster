using Deckster.Client.Common;
using Deckster.Client.Games.Common;
using Deckster.Client.Protocol;
using Deckster.Client.Serialization;

namespace Deckster.Client.Games.CrazyEights;

[JsonDerived<CrazyEightsNotification>]
public abstract class CrazyEightsNotification : IHaveDiscriminator
{
    public string Type => GetType().Name;
}

public class PlayerPutCardNotification : CrazyEightsNotification
{
    public Guid PlayerId { get; set; }
    public Card Card { get; set; }
}

public class PlayerPutEightNotification : CrazyEightsNotification
{
    public Guid PlayerId { get; set; }
    public Card Card { get; set; }
    public Suit NewSuit { get; set; }
}

public class PlayerDrewCardNotification : CrazyEightsNotification
{
    public Guid PlayerId { get; set; }
}

public class PlayerPassedNotification : CrazyEightsNotification
{
    public Guid PlayerId { get; set; }
}

public class ItsYourTurnNotification : CrazyEightsNotification
{
    public PlayerViewOfGame PlayerViewOfGame { get; init; } = new();
}

public class GameStartedNotification : CrazyEightsNotification
{
    public PlayerViewOfGame PlayerViewOfGame { get; init; } = new();
}

public class GameEndedNotification : CrazyEightsNotification
{
    public List<PlayerData> Players { get; init; } = [];
}
