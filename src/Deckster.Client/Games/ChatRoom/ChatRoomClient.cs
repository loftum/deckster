using System.Text.Json;
using Deckster.Client.Communication;

namespace Deckster.Client.Games.ChatRoom;

public class ChatRoomClient : IDisposable, IAsyncDisposable
{
    public event Action<IDecksterChannel, DecksterCommand> OnMessage;
    
    private readonly WebSocketDecksterChannel _channel;

    public ChatRoomClient(WebSocketDecksterChannel channel)
    {
        _channel = channel;
        channel.OnMessage += MessageReceived;
    }

    private void MessageReceived(IDecksterChannel channel, byte[] bytes)
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