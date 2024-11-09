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
        if (cards.Count == 0)
        {
            return false;
        }
        var previousValue = -1;
        foreach (var card in cards)
        {
            var value = card.GetValue(caluclation);
            if (previousValue == -1)
            {
                if (value != 0)
                {
                    previousValue = value;    
                }
                continue;
            }

            if (!IsOk(value))
            {
                return false;
            }
            
            previousValue++;
        }

        return true;

        bool IsOk(int value)
        {
            return value == 0 ||
                   previousValue == -1 ||
                   value == previousValue + 1;
        }
    }

    public static bool AreOfSameRank(this IList<Card> cards)
    {
        if (cards.Count == 0)
        {
            return false;
        }

        var previousRank = -1;
        
        foreach (var card in cards)
        {
            if (previousRank == -1)
            {
                previousRank = card.Rank;
                continue;
            }

            if (card.Rank != previousRank && !card.IsJoker())
            {
                return false;
            }
        }

        return true;
    }
}