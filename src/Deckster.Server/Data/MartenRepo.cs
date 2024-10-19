using Marten;

namespace Deckster.Server.Data;

public class MartenRepo : IRepo, IDisposable, IAsyncDisposable
{
    private readonly IDocumentStore _store;
    private readonly IDocumentSession _session;

    public MartenRepo(IDocumentStore store)
    {
        _store = store;
        _session = store.LightweightSession();
    }
    
    public Task<T?> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : DatabaseObject
    {
        return _session.LoadAsync<T>(id, cancellationToken);
    }

    public async Task SaveAsync<T>(T item, CancellationToken cancellationToken = default) where T : DatabaseObject
    {
        _session.Store(item);
        await _session.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<T> Query<T>() where T : DatabaseObject
    {
        return _session.Query<T>();
    }

    public IEventStream GetEventStream<T>(Guid id) where T : DatabaseObject
    {
        var session = _store.LightweightSession();
        session.Events.StartStream<T>(id);
        return new EventStream(id, session);
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

public interface IEventStream : IDisposable, IAsyncDisposable
{
    void Append(object e);
    Task SaveChangesAsync();
}

public class EventStream : IEventStream
{
    private readonly Guid _id;
    private readonly IDocumentSession _session;

    public EventStream(Guid id, IDocumentSession session)
    {
        _id = id;
        _session = session;
    }

    public void Append(object e)
    {
        _session.Events.Append(_id, e);
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