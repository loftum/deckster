using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Deckster.Communication.Handshake;
using Deckster.Core;
using Microsoft.Extensions.Logging;

namespace Deckster.Communication;

public class DecksterCommunicator : IDecksterCommunicator
{
    private readonly ILogger _logger;
    public PlayerData PlayerData { get; }
    public event Func<IDecksterCommunicator, byte[], Task>? OnMessage;
    public event Func<IDecksterCommunicator, Task>? OnDisconnected;

    private readonly Socket _readSocket;
    private readonly Stream _readStream;

    private readonly Socket _writeSocket;
    private readonly Stream _writeStream;
    
    private Task? _readTask;

    private static readonly byte[] Disconnect = "disconnect"u8.ToArray();
    
    private readonly CancellationTokenSource _cts = new();

    private bool _isConnected = true;
    
    public DecksterCommunicator(Socket readSocket, Stream readStream, Socket writeSocket, Stream writeStream, PlayerData playerData)
    {
        _logger =  Log.Factory.CreateLogger($"{nameof(DecksterCommunicator)} {playerData.PlayerId}");
        _logger.LogInformation("Helloooo!");
        _readSocket = readSocket;
        _readStream = readStream;
        _writeSocket = writeSocket;
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
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Got message {m}", Encoding.UTF8.GetString(message));    
                }

                if (message.SequenceEqual(Disconnect))
                {
                    await DoDisconnectAsync();
                    return;
                }

                var handler = OnMessage;

                if (handler != null)
                {
                    await handler(this, message);
                }
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Cancelled. Disconnecting.");
            await DisconnectAsync();
        }
        catch(SocketException e)
        {
            _logger.LogInformation("Got SocketException {code} {socketErrorCode} {message}. Disconnecting.", e.ErrorCode, e.SocketErrorCode, e.Message);
            await DisconnectAsync();
        }
        catch(Exception e)
        {
            _logger.LogInformation("Got {typ} {message}. Disconnecting.", e.GetType().Name, e.Message);
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
    
    public Task SendAsync<TRequest>(TRequest message, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        return _writeStream.SendJsonAsync(message, options, cancellationToken);
    }

    public Task<T?> ReceiveAsync<T>(JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        return _writeStream.ReceiveJsonAsync<T>(options, cancellationToken);
    }

    public Task RespondAsync<TResponse>(TResponse response, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        return _readStream.SendJsonAsync(response, options, cancellationToken);
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
            _readSocket.Disconnect(false);
            _writeSocket.Disconnect(false);
            _readStream.Dispose();
            _writeStream.Dispose();
            _cts.Dispose();    
        }
        
        GC.SuppressFinalize(this);
    }
}
