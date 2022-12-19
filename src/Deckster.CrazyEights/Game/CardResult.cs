using Deckster.Core.Domain;
using Deckster.Core.Games;

namespace Deckster.CrazyEights.Game;

public class CardResult : SuccessResult
{
    public Card Card { get; init; }

    public CardResult()
    {
        
    }

    public CardResult(Card card)
    {
        Card = card;
    }
}