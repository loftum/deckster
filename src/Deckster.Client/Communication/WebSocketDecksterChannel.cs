using System.Net;
using System.Net.WebSockets;
using Deckster.Client.Common;
using Deckster.Client.Logging;
using Microsoft.Extensions.Logging;

namespace Deckster.Client.Communication;

public class WebSocketDecksterChannel : IDecksterChannel
{
    public PlayerData PlayerData { get; }
    public event Action<IDecksterChannel, byte[]>? OnMessage;
    public event Func<IDecksterChannel, Task>? OnDisconnected;
    private readonly ClientWebSocket _socket;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger _logger;

    private Task? _readTask;

    public WebSocketDecksterChannel(ClientWebSocket socket, PlayerData playerData)
    {
        _logger =  Log.Factory.CreateLogger($"{nameof(DecksterChannel)} {playerData.Name}");
        _socket = socket;
        PlayerData = playerData;
        _readTask = ReadMessages();
    }

    private async Task ReadMessages()
    {
        var buffer = new byte[4096];
        _cts.Token.Register(() => _socket.Dispose());
        while (!_cts.Token.IsCancellationRequested && _socket.State == WebSocketState.Open)
        {
            var result = await _socket.ReceiveAsync(buffer, _cts.Token);
            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                case WebSocketMessageType.Binary:
                {
                    var bytes = new byte[result.Count];
                    Array.Copy(buffer, bytes, result.Count);
                    OnMessage?.Invoke(this, bytes);
                    break;
                }
                case WebSocketMessageType.Close:
                    OnDisconnected?.Invoke(this);
                    await _socket.CloseOutputAsync(WebSocketCloseStatus.Empty, "", _cts.Token);
                    break;
            }
        }
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", cancellationToken);
    }

    public async Task SendAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default)
    {
        await _socket.SendMessageAsync(message, cancellationToken);
    }

    public Task<T?> ReceiveAsync<T>(CancellationToken cancellationToken = default)
    {
        return _socket.ReceiveMessageAsync<T>(cancellationToken: cancellationToken);
    }

    public Task RespondAsync<TResponse>(TResponse response, CancellationToken cancellationToken = default)
    {
        return SendAsync(response, cancellationToken);
    }

    public static async Task<WebSocketDecksterChannel> ConnectAsync(Uri uri, PlayerData playerData, string token, CancellationToken cancellationToken = default)
    {
        var socket = new ClientWebSocket();
        socket.Options.SetRequestHeader("Authorization", $"Bearer {token}");
        
        await socket.ConnectAsync(uri, cancellationToken);
        return new WebSocketDecksterChannel(socket, playerData);
    }
    
    public void Dispose()
    {
        _socket.Dispose();
    }
}