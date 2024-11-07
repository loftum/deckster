using Deckster.Core.Games.Common;

namespace Deckster.Games.Yaniv;

public class YanivGameCreatedEvent : GameCreatedEvent
{
    public List<Card> Deck { get; init; }
}