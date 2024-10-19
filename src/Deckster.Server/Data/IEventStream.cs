namespace Deckster.Server.Data;

public interface IEventStream : IDisposable, IAsyncDisposable
{
    void Append(object e);
    Task SaveChangesAsync();
}