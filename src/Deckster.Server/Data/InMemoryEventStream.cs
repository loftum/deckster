namespace Deckster.Server.Data;

public class InMemoryEventStream : IEventStream
{
    public Guid Id { get; }
    public List<object> Events { get; } = [];

    public InMemoryEventStream(Guid id, IEnumerable<object> startEvents)
    {
        Id = id;
        Events.AddRange(startEvents);
    }
    
    public void Append(object e)
    {
        Events.Add(e);
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}