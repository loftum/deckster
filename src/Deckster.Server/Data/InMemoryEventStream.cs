namespace Deckster.Server.Data;

public class InMemoryEventStream : IEventStream
{
    public void Append(object e)
    {
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