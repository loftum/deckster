using Deckster.Core.Domain;

namespace Deckster.Core.Games.CrazyEights;

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