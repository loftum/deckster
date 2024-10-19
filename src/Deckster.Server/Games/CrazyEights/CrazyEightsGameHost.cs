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
        if (_game == null || _game.State == GameState.Finished)
        {
            await channel.ReplyAsync(new FailureResponse("Game is not running"), JsonOptions);
            return;
        }

        if (!_game.TryHandle(request, out var response, out var notification))
        {
            await channel.ReplyAsync(new CrazyEightsFailureResponse($"Unknown command '{request.Type}'"), JsonOptions);
            return;
        }
        
        
        
        await channel.ReplyAsync(response, JsonOptions);
        if (notification != null)
        {
            await BroadcastMessageAsync(notification);
        }
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

    public override async Task StartAsync()
    {
        if (_game != null)
        {
            return;
        }

        var startEvent = new CrazyEightsGameStartedEvent
        {
            Id = Guid.NewGuid(),
            Players = _players.Values.Select(p => p.Player).ToList(),
            Deck = Decks.Standard.KnuthShuffle(DateTimeOffset.UtcNow.Nanosecond)
        }; 
        
        _game = CrazyEightsGame.Create(startEvent);
        _events = _repo.StartEventStream<CrazyEightsGame>(_game.Id, startEvent);
        _events.Append(startEvent);
        foreach (var player in _players.Values)
        {
            player.Start<CrazyEightsRequest>(MessageReceived, JsonOptions, Cts.Token);
        }
        var currentPlayerId = _game.CurrentPlayer.Id;
        await _players[currentPlayerId].PostMessageAsync(new ItsYourTurnNotification(), JsonOptions);
    }
}