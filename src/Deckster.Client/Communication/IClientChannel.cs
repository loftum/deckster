using Deckster.Client.Common;

namespace Deckster.Client.Communication;

public interface IClientChannel : IDisposable, IAsyncDisposable
{
    
}

public interface IClientChannel<out TNotification> : IClientChannel
{
    PlayerData PlayerData { get; }
    event Action<TNotification>? OnMessage;
    event Action<string>? OnDisconnected;
    Task DisconnectAsync();
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default);
}