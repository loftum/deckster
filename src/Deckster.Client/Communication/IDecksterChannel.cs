using Deckster.Client.Common;
using Deckster.Client.Communication.Handshake;

namespace Deckster.Client.Communication;

public interface IDecksterChannel : IDisposable
{
    PlayerData PlayerData { get; }
    event Action<IDecksterChannel, byte[]>? OnMessage;
    event Func<IDecksterChannel, Task>? OnDisconnected;
    Task DisconnectAsync();
    Task SendAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default);
    Task<T?> ReceiveAsync<T>(CancellationToken cancellationToken = default);
    Task RespondAsync<TResponse>(TResponse response, CancellationToken cancellationToken = default);
}