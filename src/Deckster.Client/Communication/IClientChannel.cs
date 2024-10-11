using Deckster.Client.Common;

namespace Deckster.Client.Communication;

public interface IClientChannel : IDisposable, IAsyncDisposable
{
    PlayerData PlayerData { get; }
    event Action<string>? OnDisconnected;
    Task DisconnectAsync();
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default);
    void StartReadNotifications<TNotification>(Action<TNotification> handle);
}