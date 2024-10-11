using Deckster.Client.Common;
using Deckster.Client.Protocol;
using Deckster.Client.Serialization;

namespace Deckster.Client.Games.Uno;

[JsonDerived<UnoGameNotification>]
public abstract class UnoGameNotification: IHaveDiscriminator
{
    public string Type { get; }
}

public class PlayerPutCardNotification : UnoGameNotification
{
    public OtherUnoPlayer Player { get; set; }
    public UnoCard Card { get; set; }
}

public class PlayerPutWildNotification : UnoGameNotification
{
    public OtherUnoPlayer Player { get; set; }
    public UnoCard Card { get; set; }
    public UnoColor NewColor { get; set; }
}

public class PlayerDrewCardNotification : UnoGameNotification
{
    public OtherUnoPlayer Player { get; set; }
}

public class PlayerPassedNotification : UnoGameNotification
{
    public OtherUnoPlayer Player { get; set; }
}

public class ItsYourTurnNotification : UnoGameNotification
{
    public PlayerViewOfUnoGame PlayerViewOfGame { get; init; }
}

public class GameStartedNotification : UnoGameNotification
{
    public List<PlayerData> Players { get; init; }
}

public class GameEndedNotification : UnoGameNotification
{
    public List<PlayerData> Players { get; init; }
}

public class RoundStartedMessage
{
    public PlayerViewOfUnoGame PlayerViewOfGame { get; init; }
}

public class RoundEndedMessage
{
    public List<PlayerData> Players { get; init; }
}
