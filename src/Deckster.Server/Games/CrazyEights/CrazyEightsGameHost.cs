using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Common;
using Deckster.Client.Games.CrazyEights;
using Deckster.Server.Communication;
using Deckster.Server.Data;
using Deckster.Server.Games.ChatRoom;
using Deckster.Server.Games.Common;
using Deckster.Server.Games.CrazyEights.Core;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsGameHost : GameHost<CrazyEightsRequest, CrazyEightsResponse, CrazyEightsNotification>
{
    public event Action<IGameHost>? OnEnded;
    public override string GameType => "CrazyEights";
    public override GameState State => _game.State;

    private CrazyEightsGame? _game;
    private readonly IRepo _repo;
    private IEventStream? _events;

    public CrazyEightsGameHost(IRepo repo)
    {
        _repo = repo;
    }

    public override bool TryAddPlayer(IServerChannel channel, [MaybeNullWhen(true)] out string error)
    {
        if (_players.Count >= 4)
        {
            error = "Too many players";
            return false;
        }

        if (!_players.TryAdd(channel.Player.Id, channel))
        {
            error = "Could not add player";
            return false;
        }

        error = default;
        return true;
    }

    private async void MessageReceived(IServerChannel channel, CrazyEightsRequest request)
    {
        if (_game == null)
        {
            await channel.ReplyAsync(new FailureResponse("Game is not running"), JsonOptions);
            return;
        }

        if (!_game.TryApply(request, out var result))
        {
            await channel.ReplyAsync(new CrazyEightsFailureResponse($"Unknown command '{request.Type}'"), JsonOptions);
            return;
        }
        
        await channel.ReplyAsync(result, JsonOptions);
        if (result is CrazyEightsSuccessResponse)
        {
            if (_game.State == GameState.Finished)
            {
                await _events.SaveChangesAsync();
                await _events.DisposeAsync();
                _events = null;
                _game = null;
                
                await BroadcastMessageAsync(new GameEndedNotification());
                await Task.WhenAll(_players.Values.Select(p => p.WeAreDoneHereAsync()));
                await Cts.CancelAsync();
                Cts.Dispose();
                OnEnded?.Invoke(this);
                
                return;
            }
            var currentPlayerId = _game.CurrentPlayer.Id;
            await _players[currentPlayerId].PostMessageAsync(new ItsYourTurnNotification(), JsonOptions);
        }
    }

    // private DecksterResponse HandleRequest(IServerChannel channel, CrazyEightsRequest message)
    // {
    //     switch (message)
    //     {
    //         case PutCardRequest r:
    //             return _game.PutCard(channel.Player.Id, r.Card);
    //         case PutEightRequest r:
    //             return _game.PutEight(channel.Player.Id, r.Card, r.NewSuit);
    //         case DrawCardRequest r:
    //             return _game.DrawCard(channel.Player.Id);
    //         case PassRequest r:
    //             return _game.Pass(channel.Player.Id);
    //         default:
    //             return new CrazyEightsFailureResponse($"Unknown command '{message.Type}'");
    //     }
    // }

    public override async Task Start()
    {
        if (_game != null)
        {
            return;
        }

        var e = new CrazyEightsGameStartedEvent
        {
            Id = Guid.NewGuid(),
            Players = _players.Values.Select(p => p.Player).ToList(),
            Deck = Decks.Standard.KnuthShuffle(DateTimeOffset.UtcNow.Nanosecond)
        }; 
        
        _game = CrazyEightsGame.Create(e);
        _events = _repo.GetEventStream<CrazyEightsGame>(_game.Id);
        _events.Append(e);
        foreach (var player in _players.Values)
        {
            player.Start<CrazyEightsRequest>(MessageReceived, JsonOptions, Cts.Token);
        }
        var currentPlayerId = _game.CurrentPlayer.Id;
        await _players[currentPlayerId].PostMessageAsync(new ItsYourTurnNotification(), JsonOptions);
    }
}