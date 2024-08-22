using Deckster.Server.Data;

namespace Deckster.Server.Communication;

public interface IServerChannel : IDisposable
{
    DecksterUser User { get; }
    ValueTask ReplyAsync(object message, CancellationToken cancellationToken = default);
    ValueTask PostEventAsync(object message, CancellationToken cancellationToken = default);
    Task WeAreDoneHereAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
}