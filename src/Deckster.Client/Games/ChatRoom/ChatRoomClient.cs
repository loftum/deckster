using Deckster.Client.Communication;
using Deckster.Client.Communication.WebSockets;
using Deckster.Client.Protocol;

namespace Deckster.Client.Games.ChatRoom;

public class ChatRoomClient : IDisposable, IAsyncDisposable
{
    public event Action<IClientChannel, DecksterCommand> OnMessage;
    
    private readonly WebSocketClientChannel _channel;

    public ChatRoomClient(WebSocketClientChannel channel)
    {
        _channel = channel;
        channel.OnMessage += MessageReceived;
    }

    private void MessageReceived(IClientChannel channel, byte[] bytes)
    {
        var message = DecksterJson.Deserialize<DecksterCommand>(bytes);
        if (message == null)
        {
            return;
        }
        OnMessage.Invoke(channel, message);
    }

    public async Task<TMessage?> SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
    {
        await _channel.SendAsync(message, cancellationToken);
        return await _channel.ReceiveAsync<TMessage>(cancellationToken);
    }

    public void Dispose()
    {
        _channel.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync();
    }
}