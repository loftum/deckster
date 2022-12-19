using Deckster.Core.Domain;
using Deckster.Core.Games;

namespace Deckster.CrazyEights.Game;

public class PlayerViewOfGame : SuccessResult
{
    public List<Card> Cards { get; set; }
    public Card TopOfPile { get; set; }
    public List<OtherCrazyEightsPlayer> OtherPlayers { get; set; }
}