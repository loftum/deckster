using Deckster.Server.Games.CrazyEights.Core;
using Deckster.Server.Games.Uno.Core;

namespace Deckster.Server.Games.Uno;

public class UnoProjection : GameProjection<UnoGame>
{
    public override (UnoGame game, object startEvent) Create(IGameHost host)
    {
        var startEvent = new UnoGameCreatedEvent
        {
            Id = Guid.NewGuid(),
            Players = host.GetPlayers(),
            Deck = UnoDeck.Standard.KnuthShuffle(new Random().Next(0, int.MaxValue)),
        };

        return (UnoGame.Create(startEvent), startEvent);
    }
    
    

}