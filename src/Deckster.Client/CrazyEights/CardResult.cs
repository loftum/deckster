using Deckster.Client.Core.Domain;
using Deckster.Client.Core.Games;

namespace Deckster.Client.CrazyEights;

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