using System.Net.WebSockets;
using System.Text.Json;
using Deckster.Client.Games.CrazyEights;
using Deckster.Client.Serialization;
using Deckster.Server.Data;

namespace Deckster.Server.Games;

public class ServerChannel : IDisposable
{
    public event Action<Guid, DecksterCommand>? Received;
    
    public DecksterUser User { get; }
    private readonly WebSocket _commandSocket;
    private readonly WebSocket _eventSocket;

    private Task? _listenTask;
    
    public ServerChannel(DecksterUser user, WebSocket commandSocket, WebSocket eventSocket)
    {
        User = user;
        _commandSocket = commandSocket;
        _eventSocket = eventSocket;
    }

    public void Start(CancellationToken cancellationToken)
    {
        _listenTask = ListenAsync(cancellationToken);
    }
    
    private async Task ListenAsync(CancellationToken _cancellationToken)
    {
        try
        {
            var buffer = new byte[4096];
            while (!_cancellationToken.IsCancellationRequested)
            {
                var result = await _commandSocket.ReceiveAsync(buffer, _cancellationToken);
            
                var message = JsonSerializer.Deserialize<DecksterCommand>(new ArraySegment<byte>(buffer, 0, result.Count), Jsons.CamelCase);
                if (message != null)
                {
                    Received?.Invoke(User.Id, message);
                }
            }
        }
        catch (TaskCanceledException e)
        {
            return;
        }
    }
    
    public ValueTask ReplayAsync(object message, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(message, message.GetType(), Jsons.CamelCase);
        return _commandSocket.SendAsync(bytes, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, cancellationToken);
    }
    
    public Task WeAreDoneHereAsync(CancellationToken cancellationToken = default)
    {
        return _commandSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
    }

    public async Task DisconnectAsync()
    {
        await _commandSocket.CloseOutputAsync(WebSocketCloseStatus.ProtocolError, "Closing", default);
        await _eventSocket.CloseOutputAsync(WebSocketCloseStatus.ProtocolError, "Closing", default);
    }

    public void Dispose()
    {
        _commandSocket.Dispose();
        _eventSocket.Dispose();
        _listenTask?.Dispose();
    }
}