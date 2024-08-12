using System.Net.WebSockets;
using System.Text.Json;
using Deckster.Client.Serialization;
using Deckster.Server.Data;

namespace Deckster.Server.Games;

public class DecksterPlayer<TMessage>
{
    public Action<Guid, TMessage> Received = (_, _) => { };
    
    public DecksterUser User { get; }
    public WebSocket Socket { get; }

    private readonly CancellationToken _cancellationToken;
    private Task _listenTask;
    
    public DecksterPlayer(DecksterUser user, WebSocket socket, CancellationToken cancellationToken)
    {
        User = user;
        Socket = socket;
        _cancellationToken = cancellationToken;
        _listenTask = ListenAsync();
    }

    public ValueTask SendAsync(object message, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(message, message.GetType(), Jsons.CamelCase);
        return Socket.SendAsync(bytes, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, cancellationToken);
    }
    
    private async Task ListenAsync()
    {
        try
        {
            var buffer = new byte[4096];
            while (!_cancellationToken.IsCancellationRequested)
            {
                var result = await Socket.ReceiveAsync(buffer, _cancellationToken);
            
                var message = JsonSerializer.Deserialize<TMessage>(new ArraySegment<byte>(buffer, 0, result.Count), Jsons.CamelCase);
                if (message != null)
                {
                    Received(User.Id, message);    
                }
            }
        }
        catch (TaskCanceledException e)
        {
            return;
        }
    }

    public Task WeAreDoneHereAsync(CancellationToken cancellationToken = default)
    {
        return Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
    }
}

