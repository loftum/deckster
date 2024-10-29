using Deckster.Client.Games.ChatRoom;
using Deckster.Server.Games.Common;

namespace Deckster.Server.Games.ChatRoom;

public class Chat : GameObject
{
    public override GameState State => GameState.Running;

    public List<SendChatRequest> Transcript { get; init; } = [];

    public static Chat Create(ChatCreatedEvent e)
    {
        return new Chat
        {
            Id = e.Id,
            StartedTime = e.StartedTime,
        };
    }
    
    public async Task<ChatResponse> ChatAsync(SendChatRequest @event)
    {
        Transcript.Add(@event);
        var response = new ChatResponse();
        await Communication.RespondAsync(@event.PlayerId, response);
        await Communication.NotifyAllAsync(new ChatNotification
        {
            Sender = @event.PlayerId.ToString(),
            Message = @event.Message
        });
        
        return response;
    }

    public override Task StartAsync()
    {
        return Task.CompletedTask;
    }
}

