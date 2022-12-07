using Deckster.Core.Domain;

namespace Deckster.Core.Games.CrazyEights;

public class CrazyEightsPlayer
{
    public Guid Id { get; private init; } = Guid.NewGuid();
    public string Name { get; init; } = "";
    
    public List<Card> Cards { get; } = new();

    public static readonly CrazyEightsPlayer Null = new()
    {
         Id = Guid.Empty,
         Name = "Ing. Kognito"
    };

    public bool HasCard(Card card) => Cards.Contains(card);

    public bool IsStillPlaying() => Cards.Any();
}

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string AccessToken { get; init; } = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
    public string Name { get; init; } = "New player";
}