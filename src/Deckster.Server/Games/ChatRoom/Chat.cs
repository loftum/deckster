using Deckster.Client.Games.ChatRoom;

namespace Deckster.Server.Games.ChatRoom;

public class Chat : GameObject
{
    private IGameContext _context;
    public List<SendChatMessage> Transcript { get; init; } = [];

    public static Chat Create(ChatCreated e)
    {
        return new Chat
        {
            Id = e.Id,
            StartedTime = e.StartedTime,
            _context = e.GetContext()
        };
    }
    
    public async Task HandleAsync(SendChatMessage @event)
    {
        await Apply(@event);
        await _context.RespondAsync(@event.PlayerId, new ChatResponse());
        await _context.BroadcastNotificationAsync(new ChatNotification
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

