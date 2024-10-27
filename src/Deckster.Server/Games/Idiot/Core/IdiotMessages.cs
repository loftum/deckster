using Deckster.Client.Games.Common;
using Deckster.Client.Games.Idiot;
using Deckster.Client.Protocol;

namespace Deckster.Server.Games.Idiot.Core;

public class IdiotRequest : DecksterRequest;
public class IdiotResponse : DecksterResponse;
public class IdiotNotification : DecksterNotification;

public class PlayerPutCardsNotification : IdiotNotification
{
    public Guid PlayerId { get; init; }
    public Card[] Cards { get; init; }
}

public class DiscardPileFlushedNotification : IdiotNotification
{
    public Guid PlayerId { get; init; }
}

public class ItsYourTurnNotification : IdiotNotification
{
    public PlayerViewOfGame PlayerViewOfGame { get; init; }
}

public class GameEndedNotification : IdiotNotification;

