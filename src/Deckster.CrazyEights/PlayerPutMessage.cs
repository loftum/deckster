using Deckster.Core.Domain;

namespace Deckster.CrazyEights;

public class PlayerPutMessage
{
    public Guid PlayerId { get; set; }
    public Card Card { get; set; } 
}