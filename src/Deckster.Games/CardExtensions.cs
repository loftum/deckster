using Deckster.Core.Games.Common;

namespace Deckster.Games;

public enum ValueCaluclation
{
    AceIsFourteen,
    AceIsOne
}

public static class CardExtensions
{
    public static int GetValue(this Card card, ValueCaluclation caluclation)
    {
        return caluclation switch
        {
            ValueCaluclation.AceIsFourteen => card.Rank == 1 ? 14 : card.Rank,
            _ => card.Rank
        };
    }

    public static bool IsStraight(this IList<Card> cards, ValueCaluclation caluclation)
    {
        var previousRank = -1;
        foreach (var card in cards)
        {
            if (previousRank == -1)
            {
                previousRank = card.GetValue(caluclation);
            }
        }
    }
}