using System.Security.Cryptography;

namespace Deckster.Core.Domain;

public class Deck
{
    private readonly List<Card> _cards;

    public Deck(IEnumerable<Card> cards)
    {
        _cards = cards.ToList().KnuthShuffle();
    }

    public void Shuffle()
    {
        
    }
    
    public static Deck Default
    {
        get
        {
            var cards = Enumerable.Range(1, 13).SelectMany(rank => Enum.GetValues<Suit>().Select(suit => new Card(rank, suit)));
            return new Deck(cards);
        }
    }
}

public static class CardExtensions
{
    public static List<Card> KnuthShuffle(this List<Card> cards)
    {
        var random = new Random();
        var ii = cards.Count;
        while (ii > 1)
        {
            var k = random.Next(ii--);
            (cards[ii], cards[k]) = (cards[k], cards[ii]);
        }

        return cards;
    }
    
    public static IEnumerable<Card> Shuffle(this IEnumerable<Card> cards)
    {
        using var random = RandomNumberGenerator.Create();
        return cards.OrderBy(c => random.Next());
    }

    private static int Next(this RandomNumberGenerator random)
    {
        var bytes = new byte[4];
        random.GetBytes(bytes);
        return Convert.ToInt32(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
    }
}