using Deckster.Client.Common;

namespace Deckster.Client.Communication;

public interface IClientChannel : IDisposable, IAsyncDisposable
{
    PlayerData PlayerData { get; }
    event Action<IClientChannel, byte[]>? OnMessage;
    event Action<IClientChannel>? OnDisconnected;
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    
    Task SendAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default);
    Task<T?> ReceiveAsync<T>(CancellationToken cancellationToken = default);
}