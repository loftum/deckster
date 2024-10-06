using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Common;
using Deckster.Client.Games.CrazyEights;
using Deckster.Client.Protocol;
using Deckster.Server.Communication;
using Deckster.Server.Games.Common;
using Deckster.Server.Games.CrazyEights.Core;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsGameHost : IGameHost
{
    public event EventHandler<IGameHost>? OnEnded;

    public string GameType => "CrazyEights";
    public GameState State => _game.State;
    public string Name { get; init; }

    private readonly ConcurrentDictionary<Guid, IServerChannel> _players = new();
    private readonly CrazyEightsGame _game = new() { Id = Guid.NewGuid() };
    private readonly CancellationTokenSource _cts = new();
    
    public ICollection<PlayerData> GetPlayers()
    {
        return _players.Values.Select(c => c.Player).ToArray();
    }

    private async void MessageReceived(PlayerData player, DecksterRequest message)
    {
        if (!_players.TryGetValue(player.Id, out var channel))
        {
            return;
        }
        if (_game.State != GameState.Running)
        {
            await channel.ReplyAsync(new FailureResponse("Game is not running"));
            return;
        }

        var result = await HandleRequestAsync(player.Id, message, channel);
        if (result is SuccessResponse)
        {
            if (_game.State == GameState.Finished)
            {
                await BroadcastMessageAsync(new GameEndedNotification());
                await Task.WhenAll(_players.Values.Select(p => p.WeAreDoneHereAsync()));
                await _cts.CancelAsync();
                _cts.Dispose();
                OnEnded?.Invoke(this, this);
                return;
            }
            var currentPlayerId = _game.CurrentPlayer.Id;
            await _players[currentPlayerId].PostMessageAsync(new ItsYourTurnNotification());
        }
    }

    public bool TryAddPlayer(IServerChannel channel, [MaybeNullWhen(true)] out string error)
    {
        if (!_game.TryAddPlayer(channel.Player.Id, channel.Player.Name, out error))
        {
            error = "Could not add player";
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

    private Task BroadcastMessageAsync(DecksterNotification notification, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(_players.Values.Select(p => p.PostMessageAsync(notification, cancellationToken).AsTask()));
    }

    private async Task<DecksterResponse> HandleRequestAsync(Guid id, DecksterRequest message, IServerChannel player)
    {
        switch (message)
        {
            case PutCardRequest request:
            {
                var result = _game.PutCard(id, request.Card);
                await player.ReplyAsync(result);
                return result;
            }
            case PutEightRequest request:
            {
                var result = _game.PutEight(id, request.Card, request.NewSuit);
                await player.ReplyAsync(result);
                return result;
            }
            case DrawCardRequest:
            {
                var result = _game.DrawCard(id);
                await player.ReplyAsync(result);
                return result;
            }
            case PassRequest:
            {
                var result = _game.Pass(id);
                await player.ReplyAsync(result);
                return result;
            }
            default:
            {
                var result = new FailureResponse($"Unknown command '{message.Type}'");
                await player.ReplyAsync(result);
                return result;
            }
        }
    }

    public async Task Start()
    {
        if (_game.State != GameState.Waiting)
        {
            return;
        }
        _game.Reset();
        foreach (var player in _players.Values)
        {
            player.Received += MessageReceived;
        }
        var currentPlayerId = _game.CurrentPlayer.Id;
        await _players[currentPlayerId].PostMessageAsync(new ItsYourTurnNotification());
    }
    
    public async Task CancelAsync()
    {
        foreach (var player in _players.Values.ToArray())
        {
            player.Received -= MessageReceived;
            await player.DisconnectAsync();
            player.Dispose();
        }
    }
}