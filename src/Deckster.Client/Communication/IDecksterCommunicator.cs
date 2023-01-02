using Deckster.Client.Communication.Handshake;

namespace Deckster.Client.Communication;

public interface IDecksterCommunicator : IDisposable
{
    PlayerData PlayerData { get; }
    event Action<IDecksterCommunicator, byte[]>? OnMessage;
    event Func<IDecksterCommunicator, Task>? OnDisconnected;
    Task DisconnectAsync();
    Task SendAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default);
    Task<T?> ReceiveAsync<T>(CancellationToken cancellationToken = default);
    Task RespondAsync<TResponse>(TResponse response, CancellationToken cancellationToken = default);
}