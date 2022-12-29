using System.Collections.Concurrent;
using System.Text.Json;
using Deckster.Communication;
using Deckster.Communication.Handshake;

namespace Deckster.Server.Games.CrazyEights;

public class InProcessDecksterCommunicator : IDecksterCommunicator
{
    public InProcessDecksterCommunicator Target { get; }
    
    public PlayerData PlayerData { get; }
    public event Func<IDecksterCommunicator, byte[], Task>? OnMessage;
    public event Func<IDecksterCommunicator, Task>? OnDisconnected;

    private readonly ConcurrentQueue<object> _responses = new();

    public InProcessDecksterCommunicator(PlayerData playerData)
    {
        PlayerData = playerData;
        Target = new InProcessDecksterCommunicator(playerData, this);
    }

    private InProcessDecksterCommunicator(PlayerData playerData, InProcessDecksterCommunicator target)
    {
        PlayerData = playerData;
        Target = target;
    }
    
    public Task DisconnectAsync()
    {
        var handler = Target.OnDisconnected;
        return handler != null ? handler.Invoke(this) : Task.CompletedTask;
    }

    public async Task SendAsync<TRequest>(TRequest message, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        var handler = Target.OnMessage;
        if (handler == null)
        {
            return;
        }

        using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, message, options, cancellationToken);
        
        await handler.Invoke(Target, memoryStream.ToArray());
    }

    public async Task<T?> ReceiveAsync<T>(JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        object? val;
        while (!_responses.TryDequeue(out val))
        {
            await Task.Delay(10, cancellationToken);
        }

        if (val is T t)
        {
            return t;
        }

        return default;
    }

    public Task RespondAsync<TResponse>(TResponse response, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        _responses.Enqueue(response);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        
    }
}