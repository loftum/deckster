using Deckster.Client.Games.ChatRoom;
using Deckster.Server.Games.CrazyEights.Core;

namespace Deckster.Server.Games.ChatRoom;

public class Chat : GameObject
{
    public List<SendChatMessage> Transcript { get; init; } = [];

    public static Chat Create(ChatCreated e)
    {
        return new Chat
        {
            Id = e.Id,
            StartedTime = e.StartedTime
        };
    }
    
    public async Task HandleAsync(SendChatMessage @event, TurnContext? context)
    {
        await Apply(@event);
        context?.SetResponse(new ChatResponse());
        context?.AddNotification(new ChatNotification
        {
            Sender = @event.PlayerId.ToString(),
            Message = @event.Message
        });
    }
    
    public Task Apply(SendChatMessage @event)
    {
        Transcript.Add(@event);
        return Task.CompletedTask;
    }
}

