using Deckster.Client.Protocol;

namespace Deckster.Server.Games.ChatRoom;

public class NullContext : IGameContext
{
    public static NullContext Instance { get; } = new();
    
    public Task BroadcastNotificationAsync(DecksterNotification notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RespondAsync(Guid playerId, DecksterResponse response, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SendNotificationAsync(Guid playerId, DecksterNotification notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}