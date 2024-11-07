using Deckster.Core.Games.Common;
using Deckster.Core.Protocol;

namespace Deckster.Games.Yaniv;

public class PutCardsRequest : DecksterRequest
{
    public Card[] Cards { get; init; }
    public PickCardFrom PickCardFrom { get; init; }
}

public enum PickCardFrom
{
    StockPile,
    DiscardPile
}

public class PutCardsResponse : DecksterResponse
{
    public Card Card { get; init; }
}