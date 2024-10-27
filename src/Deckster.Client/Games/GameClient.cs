using Deckster.Client.Communication;
using Deckster.Client.Games.Common;
using Deckster.Client.Protocol;
using Deckster.Client.Serialization;

namespace Deckster.Client.Games;

public interface IGameClient : IDisposable, IAsyncDisposable;

public abstract class GameClient<TRequest, TResponse> : IGameClient 
    where TRequest : DecksterRequest
    where TResponse : DecksterResponse
{
    protected readonly IClientChannel Channel;
    public event Action<string>? Disconnected;

    protected GameClient(IClientChannel channel)
    {
        Channel = channel;
        channel.OnDisconnected += reason => Disconnected?.Invoke(reason);
        channel.StartReadNotifications<DecksterNotification>(OnNotification, DecksterJson.Options);
    }

    protected abstract void OnNotification(DecksterNotification notification);

    protected async Task<TResponse> SendAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var response = await Channel.SendAsync<DecksterResponse>(request, DecksterJson.Options, cancellationToken);
        return response switch
        {
            TResponse expected => expected,
            FailureResponse f => throw new Exception(f.Message),
            null => throw new Exception("OMG RESPAWNS IZ NULLZ"),
            _ => throw new Exception($"Unknown result '{response.GetType().Name}'")
        };
    }

    public async Task DisconnectAsync()
    {
        await Channel.DisconnectAsync();
    }
    
    protected async Task<TWanted> GetAsync<TWanted>(TRequest request, CancellationToken cancellationToken = default) where TWanted : TResponse
    {
        var response = await SendAsync(request, cancellationToken);
        return response switch
        {
            TWanted r => r,
            _ => throw new Exception($"Unexpected response '{response.GetType().Name}'")
        };
    }
    
    public void Dispose()
    {
        Channel.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await Channel.DisposeAsync();
    }
}