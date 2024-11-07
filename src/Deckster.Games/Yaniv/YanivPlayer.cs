using Deckster.Core.Games.Common;
using Deckster.Games.Collections;

namespace Deckster.Games.Yaniv;

public class YanivPlayer
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public List<Card> CardsOnHand { get; init; } = [];

    public static YanivPlayer Null => new YanivPlayer
    {
        Id = default,
        Name = "Ing. Kognito"
    };

    public bool HasCards(Card[] cards)
    {
        return CardsOnHand.ContainsAll(cards);
    }
}