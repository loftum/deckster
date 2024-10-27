using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Games.Common;
using Deckster.Client.Games.Uno;
using Deckster.Client.Protocol;
using Deckster.Server.Communication;
using Deckster.Server.Data;
using Deckster.Server.Games.ChatRoom;
using Deckster.Server.Games.Common;
using Deckster.Server.Games.Common.Fakes;
using Deckster.Server.Games.CrazyEights;
using Deckster.Server.Games.CrazyEights.Core;
using Deckster.Server.Games.Uno.Core;
using Deckster.Uno.SampleClient;

namespace Deckster.Server.Games.Uno;

public class UnoProjection : GameProjection<UnoGame>
{
    public override (UnoGame game, object startEvent) Create(IGameHost host)
    {
        throw new NotImplementedException();
    }
}

public class UnoGameHost : StandardGameHost<UnoGame>
{
    private readonly Locked<CrazyEightsGame> _game = new();
    private readonly UnoProjection _projection = new();
    public override string GameType => "Uno";
    public override GameState State => _game.Value?.State ?? GameState.Waiting;
    private IEventQueue<CrazyEightsGame>? _events;
    
    private readonly List<UnoNoob> _bots = [];

    public UnoGameHost(IRepo repo) : base(repo, new UnoProjection(), 4)
    {
    }

    protected override void ChannelDisconnected(IServerChannel channel)
    {
        
    }

    public override bool TryAddBot([MaybeNullWhen(true)] out string error)
    {
        var channel = new InMemoryChannel
        {
            Player = new PlayerData
            {
                Id = Guid.NewGuid(),
                Name = TestNames.Random()
            }
        };
        var bot = new UnoNoob(new UnoClient(channel));
        _bots.Add(bot);
        return TryAddPlayer(channel, out error);
    }
}