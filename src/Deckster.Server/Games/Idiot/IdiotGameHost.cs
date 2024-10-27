using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Games.Common;
using Deckster.Server.Communication;
using Deckster.Server.Data;
using Deckster.Server.Games.CrazyEights.Core;
using Deckster.Server.Games.Idiot.Core;

namespace Deckster.Server.Games.Idiot;

public class IdiotGameHost : StandardGameHost<IdiotGame>
{
    public override string GameType => "Idiot";
    
    public IdiotGameHost(IRepo repo) : base(repo, new IdiotProjection(), 4)
    {
    }
    
    protected override void ChannelDisconnected(IServerChannel channel)
    {
        
    }

    public override bool TryAddBot([MaybeNullWhen(true)] out string error)
    {
        error = "Bots not supported";
        return false;
    }
}

public class IdiotProjection : GameProjection<IdiotGame>
{
    public override (IdiotGame game, object startEvent) Create(IGameHost host)
    {
        var createdEvent = new IdiotGameCreatedEvent
        {
            Players = host.GetPlayers(),
            Deck = Decks.Standard.KnuthShuffle(new Random().Next(0, int.MaxValue))
        };

        var game = IdiotGame.Create(createdEvent);
        return (game, createdEvent);
    }
}

public class IdiotGameCreatedEvent : GameCreatedEvent
{
    public List<PlayerData> Players { get; init; } = [];
    public List<Card> Deck { get; init; }
} 