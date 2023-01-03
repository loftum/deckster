using System.Text.Json;
using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Client.Logging;

namespace Deckster.Server.Games.CrazyEights;

public class InProcessDecksterChannel : IDecksterChannel
{
    public InProcessDecksterChannel Target { get; }
    
    public PlayerData PlayerData { get; }
    public event Action<IDecksterChannel, byte[]>? OnMessage;
    public event Func<IDecksterChannel, Task>? OnDisconnected;

    private readonly Locked<object> _response = new();

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
        _logger.LogInformation("Receiving {val}", typeof(T).Name);
        while (!_response.TryGet(out val))
        {
            await Task.Delay(TimeSpan.FromMicroseconds(10), cancellationToken);
        }
        
        _logger.LogInformation("Got response {val}", val.GetType().Name);
        if (val is T t)
        {
            return t;
        }

        return default;
    }

    public async Task RespondAsync<TResponse>(TResponse response, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Responding {val}", response.GetType().Name);
        while (!Target._response.TrySet(response))
        {
            await Task.Delay(10, cancellationToken);
        }
        _logger.LogTrace("Done Responding {val}", response.GetType().Name);
    }

    public void Dispose()
    {
        
    }
}