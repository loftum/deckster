using System.Text.Json;
using Deckster.Communication;
using Deckster.Communication.Handshake;

namespace Deckster.Server.Games.CrazyEights;

public class InProcessDecksterCommunicator : IDecksterCommunicator
{
    public InProcessDecksterCommunicator Target { get; }
    
    public PlayerData PlayerData { get; }
    public event Func<IDecksterCommunicator, Stream, byte[], Task>? OnMessage;
    public event Func<IDecksterCommunicator, Task>? OnDisconnected;

    private readonly MemoryStream _writeStream = new();

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

    public Task SendJsonAsync<TRequest>(TRequest message, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        var handler = Target.OnMessage;
        _writeStream.Seek(0, SeekOrigin.Begin);
        JsonSerializer.Serialize(_writeStream, message, options);
        return handler == null ? Task.CompletedTask : handler.Invoke(Target, _writeStream, _writeStream.ToArray());
    }

    public Task<T?> ReceiveAsync<T>(JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        _writeStream.Seek(0, SeekOrigin.Begin);
        var message = JsonSerializer.Deserialize<T>(_writeStream, options);
        return Task.FromResult(message);
    }
    
    public void Dispose()
    {
        
    }
}