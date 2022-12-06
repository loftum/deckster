namespace Deckster.Core.Domain;

public class Card
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Rank { get; init; }
    public Suit Suit { get; init; }

    public Card()
    {
        
    }

    public Card(int rank, Suit suit)
    {
        if (rank is < 0 or > 13)
        {
            throw new ArgumentOutOfRangeException(nameof(rank), "Invalid rank '{rank}'");
        }
        Rank = rank;
        Suit = suit;
    }

    protected bool Equals(Card? other)
    {
        return other != null && Rank == other.Rank && Suit == other.Suit;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Card);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Rank, (int) Suit);
    }

    public override string ToString()
    {
        return $"{Map(Rank)} of {Suit}";
    }

    private static string Map(int rank)
    {
        return rank switch
        {
            0 => "Joker",
            1 => "Ace",
            >= 2 and < 11 => $"{rank}",
            11 => "Jack",
            12 => "Queen",
            13 => "King",
            _ => throw new ArgumentOutOfRangeException(nameof(rank), "Invalid rank '{rank}'")
        };
    }
}