namespace Deckster.Server.Games.ChatRoom;

public class ChatCreated
{
    private IGameContext _context = NullContext.Instance;
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset StartedTime { get; init; } = DateTimeOffset.UtcNow;

    public ChatCreated WithContext(IGameContext context)
    {
        _context = context;
        return this;
    }

    public IGameContext GetContext() => _context;
}