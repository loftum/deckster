using Deckster.Client.Protocol;

namespace Deckster.Server.Games.ChatRoom;

public interface IGameContext
{
    Task BroadcastNotificationAsync(DecksterNotification notification, CancellationToken cancellationToken = default);
    Task RespondAsync(Guid playerId, DecksterResponse response, CancellationToken cancellationToken = default);
    Task SendNotificationAsync(Guid playerId, DecksterNotification notification, CancellationToken cancellationToken = default);
}