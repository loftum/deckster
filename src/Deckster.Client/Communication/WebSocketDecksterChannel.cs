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
    private readonly ClientWebSocket _commandSocket;
    private readonly ClientWebSocket _eventSocket;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger _logger;

    private Task? _readTask;

    public WebSocketDecksterChannel(ClientWebSocket commandSocket, ClientWebSocket eventSocket, PlayerData playerData)
    {
        _logger =  Log.Factory.CreateLogger($"{nameof(DecksterChannel)} {playerData.Name}");
        _commandSocket = commandSocket;
        PlayerData = playerData;
        _eventSocket = eventSocket;
        _readTask = ReadMessages();
    }

    private async Task ReadMessages()
    {
        var buffer = new byte[4096];
        _cts.Token.Register(() => _commandSocket.Dispose());
        while (!_cts.Token.IsCancellationRequested && _commandSocket.State == WebSocketState.Open)
        {
            var result = await _eventSocket.ReceiveAsync(buffer, _cts.Token);
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
                    await _eventSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, "", _cts.Token);
                    break;
            }
        }
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(
            _eventSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", cancellationToken),
            _commandSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", cancellationToken)
        );
    }

    public async Task SendAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default)
    {
        await _commandSocket.SendMessageAsync(message, cancellationToken);
    }

    public Task<T?> ReceiveAsync<T>(CancellationToken cancellationToken = default)
    {
        return _commandSocket.ReceiveMessageAsync<T>(cancellationToken: cancellationToken);
    }

    public Task RespondAsync<TResponse>(TResponse response, CancellationToken cancellationToken = default)
    {
        return SendAsync(response, cancellationToken);
    }

    public static async Task<WebSocketDecksterChannel> ConnectAsync(Uri uri, Guid gameId, string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var joinUri = uri.ToWebSocket($"join/{gameId}");
            var commandSocket = new ClientWebSocket();
            commandSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");
            await commandSocket.ConnectAsync(joinUri, cancellationToken);
            var connectMessage = await commandSocket.ReceiveMessageAsync<ConnectMessage>(cancellationToken);
            
            var eventSocket = new ClientWebSocket();
            eventSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");
            await eventSocket.ConnectAsync(uri.ToWebSocket($"finishjoin/{connectMessage.ConnectionId}"), cancellationToken);
        
            return new WebSocketDecksterChannel(commandSocket, eventSocket, connectMessage.PlayerData);
        }
        catch (WebSocketException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public void Dispose()
    {
        _eventSocket.Dispose();
        _commandSocket.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _commandSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);
        await CastAndDispose(_commandSocket);
        await _eventSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", default);
        await CastAndDispose(_eventSocket);
        await CastAndDispose(_cts);
        if (_readTask != null) await CastAndDispose(_readTask);

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
                await resourceAsyncDisposable.DisposeAsync();
            else
                resource.Dispose();
        }
    }
}

public class ConnectMessage
{
    public PlayerData PlayerData { get; set; }
    public Uri FinishUri { get; set; }
    public Guid ConnectionId { get; set; }
}