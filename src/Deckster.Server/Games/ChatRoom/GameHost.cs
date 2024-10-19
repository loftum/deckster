using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Deckster.Client.Common;
using Deckster.Client.Protocol;
using Deckster.Client.Serialization;
using Deckster.Server.Communication;
using Deckster.Server.Games.Common;

namespace Deckster.Server.Games.ChatRoom;

public abstract class GameHost<TRequest, TResponse, TNotification> : IGameHost, IGameContext
    where TRequest : DecksterRequest
    where TResponse : DecksterResponse
    where TNotification : DecksterNotification
{
    public event Action<IGameHost>? OnEnded;
    
    public abstract string GameType { get; }
    public string Name { get; set; }
    public abstract GameState State { get; }
    
    protected readonly ConcurrentDictionary<Guid, IServerChannel> _players = new();
    protected readonly CancellationTokenSource Cts = new();

    protected readonly JsonSerializerOptions JsonOptions = DecksterJson.Create(o =>
    {
        o.AddAll<TRequest>().AddAll<TResponse>().AddAll<TNotification>();
    });

    public abstract Task StartAsync();

    public abstract bool TryAddPlayer(IServerChannel channel, [MaybeNullWhen(true)] out string error);
    

    public ICollection<PlayerData> GetPlayers()
    {
        return _players.Values.Select(c => c.Player).ToArray();
    }

    public Task BroadcastNotificationAsync(DecksterNotification notification, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(_players.Values.Select(p => p.SendNotificationAsync(notification, JsonOptions, cancellationToken).AsTask()));
    }

    public async Task RespondAsync(Guid playerId, DecksterResponse response, CancellationToken cancellationToken = default)
    {
        if (_players.TryGetValue(playerId, out var channel))
        {
            await channel.ReplyAsync(response, JsonOptions, cancellationToken);
        }
    }

    public async Task SendNotificationAsync(Guid playerId, DecksterNotification notification, CancellationToken cancellationToken = default)
    {
        if (_players.TryGetValue(playerId, out var channel))
        {
            await channel.SendNotificationAsync(notification, JsonOptions, cancellationToken);
        }
    }

    public async Task CancelAsync()
    {
        await Cts.CancelAsync();
        foreach (var player in _players.Values.ToArray())
        {
            await player.DisconnectAsync();
            player.Dispose();
        }
    }
}

