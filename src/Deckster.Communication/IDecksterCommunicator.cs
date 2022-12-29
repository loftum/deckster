using System.Text.Json;
using Deckster.Communication.Handshake;

namespace Deckster.Communication;

public interface IDecksterCommunicator : IDisposable
{
    PlayerData PlayerData { get; }
    event Func<IDecksterCommunicator, byte[], Task>? OnMessage;
    event Func<IDecksterCommunicator, Task>? OnDisconnected;
    Task DisconnectAsync();
    Task SendAsync<TRequest>(TRequest message, JsonSerializerOptions options, CancellationToken cancellationToken = default);
    Task<T?> ReceiveAsync<T>(JsonSerializerOptions options, CancellationToken cancellationToken = default);
    Task RespondAsync<TResponse>(TResponse response, JsonSerializerOptions options, CancellationToken cancellationToken = default);
}