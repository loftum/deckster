using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Communication;
using Deckster.Client.Protocol;
using Deckster.Server.Communication;
using Deckster.Server.Games.CrazyEights;

namespace Deckster.Server.Games.TestGame;

public class ChatRoomHost : IGameHost
{
    public event EventHandler<CrazyEightsGameHost>? OnEnded;
    public Guid Id { get; } = Guid.NewGuid();

    private readonly ConcurrentDictionary<Guid, WebSocketServerChannel> _players = new();
    
    public Task Start()
    {
        return Task.CompletedTask;
    }

    private async void MessageReceived(Guid id, DecksterCommand command)
    {
        Console.WriteLine($"Received: {command.Pretty()}");
        await _players[id].ReplyAsync(command);
        await BroadcastAsync(command);
    }
    
    private Task BroadcastAsync(object message, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(_players.Values.Select(p => p.PostEventAsync(message, cancellationToken).AsTask()));
    }

    public bool TryAddPlayer(WebSocketServerChannel player, [MaybeNullWhen(true)] out string error)
    {
        if (!_players.TryAdd(player.User.Id, player))
        {
            Console.WriteLine($"Could not add player {player.User.Name}");
            error = "Could not add player";
            return false;
        }
        
        Console.WriteLine($"Added player {player.User.Name}");

        player.Received += MessageReceived;
        player.Start(default);

        error = default;
        return true;
    }

    public async Task CancelAsync()
    {
        foreach (var player in _players.Values.ToArray())
        {
            await player.DisconnectAsync();
            player.Dispose();
        }
    }
}