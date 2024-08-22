using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Common;
using Deckster.Client.Games.CrazyEights;
using Deckster.Client.Protocol;
using Deckster.Server.Communication;
using Deckster.Server.Games.CrazyEights.Core;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsGameHost : IGameHost
{
    public event EventHandler<CrazyEightsGameHost> OnEnded;

    public Guid Id => _game.Id;

    private readonly ConcurrentDictionary<Guid, WebSocketServerChannel> _players = new();
    private readonly CrazyEightsGame _game = new() { Id = Guid.NewGuid() };
    private readonly CancellationTokenSource _cts = new();

    private async void MessageReceived(Guid id, DecksterCommand message)
    {
        if (!_players.TryGetValue(id, out var player))
        {
            return;
        }
        if (_game.State != GameState.Running)
        {
            await player.ReplyAsync(new FailureResult("Game is not running"));
            return;
        }

        var result = await ExecuteCommandAsync(id, message, player);
        if (result is SuccessResult)
        {
            if (_game.State == GameState.Finished)
            {
                await BroadcastAsync(new GameEndedMessage());
                await Task.WhenAll(_players.Values.Select(p => p.WeAreDoneHereAsync()));
                await _cts.CancelAsync();
                _cts.Dispose();
                OnEnded?.Invoke(this, this);
            }
            var currentPlayerId = _game.CurrentPlayer.Id;
            await _players[currentPlayerId].ReplyAsync(new ItsYourTurnMessage());
        }
    }

    public bool TryAddPlayer(WebSocketServerChannel player, [MaybeNullWhen(true)] out string error)
    {
        if (!_game.TryAddPlayer(player.User.Id, player.User.Name, out error))
        {
            error = "Could not add player";
            return false;
        }

        if (!_players.TryAdd(player.User.Id, player))
        {
            error = "Could not add player";
            return false;
        }

        error = default;
        return true;
    }

    private Task BroadcastAsync(object message, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(_players.Values.Select(p => p.ReplyAsync(message, cancellationToken).AsTask()));
    }

    private async Task<DecksterCommandResult> ExecuteCommandAsync(Guid id, DecksterCommand message, WebSocketServerChannel player)
    {
        switch (message)
        {
            case PutCardCommand command:
            {
                var result = _game.PutCard(id, command.Card);
                await player.ReplyAsync(result);
                return result;
            }
            case PutEightCommand command:
            {
                var result = _game.PutEight(id, command.Card, command.NewSuit);
                await player.ReplyAsync(result);
                return result;
            }
            case DrawCardCommand:
            {
                var result = _game.DrawCard(id);
                await player.ReplyAsync(result);
                return result;
            }
            case PassCommand:
            {
                var result = _game.Pass(id);
                await player.ReplyAsync(result);
                return result;
            }
            default:
            {
                var result = new FailureResult($"Unknown command '{message.Type}'");
                await player.ReplyAsync(result);
                return result;
            }
        }
    }

    public async Task Start()
    {
        _game.Reset();
        foreach (var player in _players.Values)
        {
            player.Received += MessageReceived;
        }
        var currentPlayerId = _game.CurrentPlayer.Id;
        await _players[currentPlayerId].ReplyAsync(new ItsYourTurnMessage());
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