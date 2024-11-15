using Deckster.Core.Games.Common;

namespace Deckster.Games.CrazyEights;

public class CrazyEightsPlayer
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public List<Card> Cards { get; init; } = [];

    public static readonly CrazyEightsPlayer Null = new()
    {
         Id = Guid.Empty,
         Name = "Ing. Kognito"
    };

    public bool HasCard(Card card) => Cards.Contains(card);

    public bool IsStillPlaying() => Cards.Any();
}