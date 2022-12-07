using Deckster.Core.Domain;

namespace Deckster.Core.Games.CrazyEights;

public class PlayerViewOfGame : SuccessResult
{
    public List<Card> Cards { get; set; }
    public Card TopOfPile { get; set; }
    public List<OtherCrazyEightsPlayer> OtherPlayers { get; set; }
}