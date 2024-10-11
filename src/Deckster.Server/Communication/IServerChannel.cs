using Deckster.Client.Common;
using Deckster.Client.Protocol;

namespace Deckster.Server.Communication;

public interface IServerChannel : IDisposable
{
    event Action<IServerChannel> Disconnected;
    
    PlayerData Player { get; }
    ValueTask ReplyAsync<TResponse>(TResponse response, CancellationToken cancellationToken = default);
    ValueTask PostMessageAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default);
    Task WeAreDoneHereAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    
    void Start<TRequest>(Action<PlayerData, TRequest> handle, CancellationToken cancellationToken);
}