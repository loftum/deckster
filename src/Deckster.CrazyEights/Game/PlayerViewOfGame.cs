using Deckster.Core.Domain;
using Deckster.Core.Games;

namespace Deckster.CrazyEights.Game;

public class PlayerViewOfGame : SuccessResult
{
    public List<Card> Cards { get; init; }
    public Card TopOfPile { get; init; }
    public Suit CurrentSuit { get; init; }
    public List<OtherCrazyEightsPlayer> OtherPlayers { get; init; }
}