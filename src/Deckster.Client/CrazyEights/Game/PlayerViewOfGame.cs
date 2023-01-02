using Deckster.Client.Core.Domain;
using Deckster.Client.Core.Games;

namespace Deckster.Client.CrazyEights.Game;

public class PlayerViewOfGame : SuccessResult
{
    public List<Card> Cards { get; init; }
    public Card TopOfPile { get; init; }
    public Suit CurrentSuit { get; init; }
    public List<OtherCrazyEightsPlayer> OtherPlayers { get; init; }
}