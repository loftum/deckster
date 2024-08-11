using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using Deckster.Client.Common;
using Deckster.Client.Games.CrazyEights;
using Deckster.Server.Data;
using Deckster.Server.Games.CrazyEights.Core;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsGameHost
{
    public event EventHandler<CrazyEightsGameHost> OnEnded;

    public Guid Id => _game.Id;

    private readonly ConcurrentDictionary<Guid, DecksterPlayer<CrazyEightsCommand>> _players = new();
    private readonly CrazyEightsGame _game = new() { Id = Guid.NewGuid() };
    private readonly CancellationTokenSource _cts = new();

    public bool TryAddPlayer(DecksterUser user, WebSocket socket, [MaybeNullWhen(true)] out string reason)
    {
        
        if (!_game.TryAddPlayer(user.Id, user.Name, out reason))
        {
            return false;
        }

        var player = new DecksterPlayer<CrazyEightsCommand>(user, socket, _cts.Token)
        {
            Received = MessageReceived
        };

        if (!_players.TryAdd(player.User.Id, player))
        {
            reason = "Player already exists";
            _game.RemovePlayer(player.User.Id);
            return false;
        }

        return true;
    }

    private async void MessageReceived(Guid id, CrazyEightsCommand message)
    {
        if (!_players.TryGetValue(id, out var player))
        {
            return;
        }
        if (_game.State != GameState.Running)
        {
            await player.SendAsync(new FailureResult("Game is not running"));
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
            await _players[currentPlayerId].SendAsync(new ItsYourTurnMessage());
        }
    }

    private Task BroadcastAsync(object message, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(_players.Values.Select(p => p.SendAsync(message, cancellationToken).AsTask()));
    }

    private async Task<CommandResult> ExecuteCommandAsync(Guid id, CrazyEightsCommand message, DecksterPlayer<CrazyEightsCommand> player)
    {
        switch (message)
        {
            case PutCardCommand command:
            {
                var result = _game.PutCard(id, command.Card);
                await player.SendAsync(result);
                return result;
            }
            case PutEightCommand command:
            {
                var result = _game.PutEight(id, command.Card, command.NewSuit);
                await player.SendAsync(result);
                return result;
            }
            case DrawCardCommand:
            {
                var result = _game.DrawCard(id);
                await player.SendAsync(result);
                return result;
            }
            case PassCommand:
            {
                var result = _game.Pass(id);
                await player.SendAsync(result);
                return result;
            }
            default:
            {
                var result = new FailureResult($"Unknown command '{message.Type}'");
                await player.SendAsync(result);
                return result;
            }
        }
    }


    public async Task Start()
    {
        _game.Reset();
        var currentPlayerId = _game.CurrentPlayer.Id;
        await _players[currentPlayerId].SendAsync(new ItsYourTurnMessage());
    }
}