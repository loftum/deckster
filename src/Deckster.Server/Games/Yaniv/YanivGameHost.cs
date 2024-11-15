using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Games.Yaniv;
using Deckster.Core.Games.Common;
using Deckster.Core.Games.Yaniv;
using Deckster.Games;
using Deckster.Games.Yaniv;
using Deckster.Server.Data;
using Deckster.Server.Games.Common.Fakes;
using Deckster.Yaniv.SampleClient;

namespace Deckster.Server.Games.Yaniv;

public class YanivGameHost : StandardGameHost<YanivGame>
{
    private readonly List<YanivPoorAi> _bots = [];
    
    public YanivGameHost(IRepo repo, ILoggerFactory loggerFactory) : base(repo, loggerFactory, new YanivProjection(), 5)
    {
    }

    public override string GameType => "Yaniv";

    public override bool TryAddBot([MaybeNullWhen(true)] out string error)
    {
        var channel = new InMemoryChannel
        {
            Player = new PlayerData
            {
                Id = Guid.NewGuid(),
                Name = TestUserNames.Random()
            }
        };
        var bot = new YanivPoorAi(new YanivClient(channel), LoggerFactory.CreateLogger($"Yaniv {channel.Player.Name}"));
        _bots.Add(bot);
        return TryAddPlayer(channel, out error);
    }
}

public class YanivProjection : GameProjection<YanivGame>
{
    public YanivGame Create(YanivGameCreatedEvent created)
    {
        var game = YanivGame.Instantiate(created);
        game.Deal();
        return game;
    }
    
    public override (YanivGame game, object startEvent) Create(IGameHost host)
    {
        var createdEvent = new YanivGameCreatedEvent
        {
            Id = Guid.NewGuid(),
            Name = host.Name,
            Deck = Decks.Standard().WithJokers(2).KnuthShuffle(new Random().Next(0, int.MaxValue)),
            Players = host.GetPlayers()
        };
        var game = Create(createdEvent);
        return (game, createdEvent);
    }

    public Task Apply(PutCardsRequest @event, YanivGame game) => game.PutCards(@event);
    public Task Apply(CallYanivRequest @event, YanivGame game) => game.CallYaniv(@event);
}