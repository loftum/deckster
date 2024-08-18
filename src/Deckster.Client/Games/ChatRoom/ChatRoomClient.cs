using Deckster.Client.Communication;

namespace Deckster.Client.Games.ChatRoom;

public class ChatRoomClient : IDisposable, IAsyncDisposable
{
    private readonly WebSocketDecksterChannel _channel;

    public ChatRoomClient(WebSocketDecksterChannel channel)
    {
        _channel = channel;
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