using System.Collections.Concurrent;
using System.Text.Json;
using Deckster.Client;
using Deckster.Client.Communication;
using Deckster.Client.Communication.Handshake;
using Deckster.Client.Core;
using Deckster.Client.Logging;

namespace Deckster.Server.Games.CrazyEights;

public class InProcessDecksterChannel : IDecksterChannel
{
    public InProcessDecksterChannel Target { get; }
    
    public PlayerData PlayerData { get; }
    public event Action<IDecksterChannel, byte[]>? OnMessage;
    public event Func<IDecksterChannel, Task>? OnDisconnected;

    private readonly ConcurrentQueue<object> _responses = new();

    private readonly ILogger _logger;

    public InProcessDecksterChannel(PlayerData playerData)
    {
        PlayerData = playerData;
        Target = new InProcessDecksterChannel(playerData, this);
        _logger = Log.Factory.CreateLogger($"{playerData.Name} (client)");
    }

    private InProcessDecksterChannel(PlayerData playerData, InProcessDecksterChannel target)
    {
        PlayerData = playerData;
        Target = target;
        _logger = Log.Factory.CreateLogger($"{playerData.Name} (target)");
    }
    
    public Task DisconnectAsync()
    {
        var handler = Target.OnDisconnected;
        return handler != null ? handler.Invoke(this) : Task.CompletedTask;
    }

    public async Task SendAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default)
    {
        var handler = Target.OnMessage;
        if (handler == null)
        {
            return;
        }

        using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, message, DecksterJson.Options, cancellationToken);
        handler.Invoke(Target, memoryStream.ToArray());
    }

    public async Task<T?> ReceiveAsync<T>(CancellationToken cancellationToken = default)
    {
        object? val;
        while (!_responses.TryDequeue(out val))
        {
            _logger.LogTrace("Waiting for {val}", typeof(T).Name);
            await Task.Delay(10, cancellationToken);
        }
        
        _logger.LogTrace("Got response {val}", val.GetType().Name);
        if (val is T t)
        {
            return t;
        }

        return default;
    }

    public Task RespondAsync<TResponse>(TResponse response, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Responding {val}", response.GetType().Name);
        Target._responses.Enqueue(response);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        
    }
}