using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Client.Games.CrazyEights;
using Deckster.Server.Games.CrazyEights.Core;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsGameHost : IGameHost
{
    public event EventHandler<CrazyEightsGameHost> OnEnded;

    public Guid Id => _game.Id;

    private readonly ConcurrentDictionary<Guid, ServerChannel> _players = new();
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
            await player.ReplayAsync(new FailureResult("Game is not running"));
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
            await _players[currentPlayerId].ReplayAsync(new ItsYourTurnMessage());
        }
    }

    public bool TryAddPlayer(ServerChannel player, [MaybeNullWhen(true)] out string error)
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
        return Task.WhenAll(_players.Values.Select(p => p.ReplayAsync(message, cancellationToken).AsTask()));
    }

    private async Task<CommandResult> ExecuteCommandAsync(Guid id, DecksterCommand message, ServerChannel player)
    {
        switch (message)
        {
            case PutCardCommand command:
            {
                var result = _game.PutCard(id, command.Card);
                await player.ReplayAsync(result);
                return result;
            }
            case PutEightCommand command:
            {
                var result = _game.PutEight(id, command.Card, command.NewSuit);
                await player.ReplayAsync(result);
                return result;
            }
            case DrawCardCommand:
            {
                var result = _game.DrawCard(id);
                await player.ReplayAsync(result);
                return result;
            }
            case PassCommand:
            {
                var result = _game.Pass(id);
                await player.ReplayAsync(result);
                return result;
            }
            default:
            {
                var result = new FailureResult($"Unknown command '{message.Type}'");
                await player.ReplayAsync(result);
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
        await _players[currentPlayerId].ReplayAsync(new ItsYourTurnMessage());
    }
}