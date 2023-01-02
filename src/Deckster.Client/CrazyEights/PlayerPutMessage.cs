using Deckster.Client.Core.Domain;

namespace Deckster.Client.CrazyEights;

public class PlayerPutMessage
{
    public Guid PlayerId { get; set; }
    public Card Card { get; set; } 
}