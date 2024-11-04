using Deckster.Core.Games.ChatRoom;

namespace Deckster.Server.Games.ChatRoom;

public class ChatRoomProjection : GameProjection<ChatRoom>
{
    public override (ChatRoom game, object startEvent) Create(IGameHost host)
    {
        var started = new ChatCreatedEvent();

        var chat = ChatRoom.Create(started);
        chat.RespondAsync = host.RespondAsync;
        chat.PlayerSaid += host.NotifyAllAsync;
        
        return (chat, started);
    }

    public Task Apply(SendChatRequest @event, ChatRoom chatRoom) => chatRoom.ChatAsync(@event);
}