using Deckster.Server.Games;

namespace Deckster.Server.Data;

public interface IEventThing<T> : IDisposable, IAsyncDisposable where T : GameObject
{
    void Append(object e);
    Task SaveChangesAsync();
}