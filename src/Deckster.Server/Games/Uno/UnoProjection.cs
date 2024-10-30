using Deckster.Client.Games.Uno;

namespace Deckster.Server.Games.Uno;

public class UnoProjection : GameProjection<UnoGame>
{
    public UnoGame Create(UnoGameCreatedEvent created)
    {
        return UnoGame.Create(created);
    }
    
    public override (UnoGame game, object startEvent) Create(IGameHost host)
    {
        var startEvent = new UnoGameCreatedEvent
        {
            Id = Guid.NewGuid(),
            Players = host.GetPlayers(),
            Deck = UnoDeck.Standard.KnuthShuffle(new Random().Next(0, int.MaxValue)),
        };

        var game = Create(startEvent);
        game.RespondAsync = host.RespondAsync;
        game.PlayerPutCard += host.NotifyAllAsync;
        game.PlayerPutWild += host.NotifyAllAsync;
        game.PlayerDrewCard += host.NotifyAllAsync;
        game.PlayerPassed += host.NotifyAllAsync;
        game.ItsYourTurn += host.NotifyPlayerAsync;
        game.GameStarted += host.NotifyPlayerAsync;
        game.GameEnded += host.NotifyAllAsync;
        
        return (game, startEvent);
    }

    public Task Apply(PutCardRequest @event, UnoGame game) => game.PutCard(@event);
    public Task Apply(PutWildRequest @event, UnoGame game) => game.PutWild(@event);
    public Task Apply(DrawCardRequest @event, UnoGame game) => game.DrawCard(@event);
    public Task Apply(PassRequest @event, UnoGame game) => game.Pass(@event);

}