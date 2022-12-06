namespace Deckster.Core.Domain;

public class Player
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; init; }
    public List<Card> Cards { get; } = new();
}