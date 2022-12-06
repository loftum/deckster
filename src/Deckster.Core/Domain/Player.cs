namespace Deckster.Core.Domain;

public class Player
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public string Name { get; init; }
    public List<Card> Cards { get; } = new();

    public static readonly Player Null = new()
    {
        Id = Guid.Empty,
        Name = "Null"
    };
}