using Deckster.Client.Communication;

namespace Deckster.Client.Games.ChatRoom;

public class ChatRoomClient : GameClient<ChatRequest, ChatResponse, ChatNotification>
{
    public event Action<ChatNotification>? OnMessage;
    public event Action<string>? OnDisconnected;

    public ChatRoomClient(IClientChannel<ChatRequest, ChatResponse, ChatNotification> channel) : base(channel)
    {
        channel.OnMessage += MessageReceived;
        channel.OnDisconnected += s => OnDisconnected(s);
    }

    private void MessageReceived(ChatNotification notification)
    {
        OnMessage?.Invoke(notification);
    }

    public Task<ChatResponse> SendAsync(ChatRequest message, CancellationToken cancellationToken = default)
    {
        return base.SendAsync(message, cancellationToken);
    }
}