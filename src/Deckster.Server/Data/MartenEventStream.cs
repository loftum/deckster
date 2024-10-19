using Marten;

namespace Deckster.Server.Data;

public class MartenEventStream : IEventStream
{
    private readonly Guid _id;
    private readonly IDocumentSession _session;

    public MartenEventStream(Guid id, IDocumentSession session)
    {
        _id = id;
        _session = session;
    }

    public void Append(object e)
    {
        _session.Events.Append(_id, e);
    }

    public async Task Hest<T>(Guid id) where T : class
    {
        var stream = await _session.Events.AggregateStreamAsync<T>(id);
        
    }

    public Task SaveChangesAsync()
    {
        return _session.SaveChangesAsync();
    }

    public void Dispose()
    {
        _session.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _session.DisposeAsync();
    }
}