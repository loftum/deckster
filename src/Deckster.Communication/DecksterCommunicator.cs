using System.Text;
using System.Text.Json;
using Deckster.Communication.Handshake;
using Deckster.Core;
using Microsoft.Extensions.Logging;

namespace Deckster.Communication;

public class DecksterCommunicator : IDisposable
{
    private readonly ILogger _logger = Log.Factory.CreateLogger<DecksterCommunicator>();
    public PlayerData PlayerData { get; }
    public event Func<byte[], Task>? OnMessage;
    public event Func<DecksterCommunicator, Task>? OnDisconnected;
    
    private readonly Stream _readStream;
    private readonly Stream _writeStream;
    
    private Task? _readTask;

    private static readonly byte[] Disconnect = "disconnect"u8.ToArray();
    
    private readonly CancellationTokenSource _cts = new();

    private bool _isConnected = true;
    
    public DecksterCommunicator(Stream readStream, Stream writeStream, PlayerData playerData)
    {
        _logger.LogInformation("Helloooo!");
        _readStream = readStream;
        _writeStream = writeStream;
        PlayerData = playerData;
        _readTask = ReadMessages();
    }

    private async Task ReadMessages()
    {
        try
        {
            _logger.LogInformation("Reading messages");
            while (!_cts.Token.IsCancellationRequested)
            {
                var message = await _readStream.ReceiveMessageAsync(_cts.Token);
                _logger.LogInformation("Got message {m}", Encoding.UTF8.GetString(message));

                if (message.SequenceEqual(Disconnect))
                {
                    await DoDisconnectAsync();
                    return;
                }
                
                if (OnMessage != null)
                {
                    await OnMessage(message);
                }
            }
        }
        catch (TaskCanceledException)
        {
            await DisconnectAsync();
        }
    }

    private async ValueTask DoDisconnectAsync()
    {
        _logger.LogInformation("Disconnecting");
        await _readStream.DisposeAsync();
        await _writeStream.DisposeAsync();
        var handler = OnDisconnected;
        if (handler != null)
        {
            await handler(this);
        }

        _isConnected = false;
    }
    
    public async Task DisconnectAsync()
    {
        await _writeStream.SendMessageAsync(Disconnect);
        await DoDisconnectAsync();
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing");
        if (_isConnected)
        {
            _cts.Cancel();
            _readTask = null;
            _readStream.Dispose();
            _writeStream.Dispose();
            _cts.Dispose();    
        }
        
        GC.SuppressFinalize(this);
    }

    public Task SendJsonAsync<TRequest>(TRequest message, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        return _writeStream.SendJsonAsync(message, options, cancellationToken);
    }

    public Task<T?> ReceiveAsync<T>(JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        return _writeStream.ReceiveJsonAsync<T>(options, cancellationToken);
    }
}
