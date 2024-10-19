namespace Deckster.Server.Games.ChatRoom;

public class ChatCreated
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset StartedTime { get; init; } = DateTimeOffset.UtcNow;
}