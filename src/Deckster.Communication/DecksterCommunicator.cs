using System.Text.Json;
using Deckster.Communication.Handshake;

namespace Deckster.Communication;

public class DecksterCommunicator : IDisposable
{
    public PlayerData PlayerData { get; }
    public event Func<byte[], Task>? OnMessage;
    public event Func<DecksterCommunicator, Task> OnDisconnected;
    
    private readonly Stream _readStream;
    private readonly Stream _writeStream;
    
    private Task? _readTask;
    
    private readonly CancellationTokenSource _cts = new();

    public DecksterCommunicator(Stream readStream, Stream writeStream, PlayerData playerData)
    {
        _readStream = readStream;
        _writeStream = writeStream;
        PlayerData = playerData;
        _readTask = ReadMessages();
    }

    public async Task<byte[]> SendAsync(byte[] message, CancellationToken cancellationToken = default)
    {
        await _writeStream.SendMessageAsync(message, cancellationToken);
        return await _writeStream.ReceiveMessageAsync(cancellationToken);
    }

    private async Task ReadMessages()
    {
        try
        {
            while (_cts.Token.IsCancellationRequested)
            {
                var message = await _readStream.ReceiveMessageAsync(_cts.Token);
                
                if (OnMessage != null)
                {
                    await OnMessage(message);
                }
            }
        }
        catch (TaskCanceledException)
        {
            return;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _readTask = null;
        _readStream.Dispose();
        _writeStream.Dispose();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<TResult?> SendJsonAsync<TRequest, TResult>(TRequest message, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, message, options, cancellationToken);
        var bytes = await SendAsync(stream.ToArray(), cancellationToken);
        return JsonSerializer.Deserialize<TResult>(bytes, options);
    }
}
