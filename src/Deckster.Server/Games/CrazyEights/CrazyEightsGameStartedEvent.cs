using Deckster.Core.Games.Common;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsGameCreatedEvent : GameCreatedEvent
{
    public List<PlayerData> Players { get; init; } = [];
    public List<Card> Deck { get; init; } = [];
}
