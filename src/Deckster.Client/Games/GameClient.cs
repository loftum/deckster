using System.Text.Json;
using Deckster.Client.Common;
using Deckster.Client.Communication;

namespace Deckster.Client.Games;

public abstract class GameClient<TRequest, TResponse, TNotification> : IDisposable, IAsyncDisposable
{
    protected readonly IClientChannel Channel;
    public event Action<string>? Disconnected;

    protected GameClient(IClientChannel channel)
    {
        Channel = channel;
        channel.OnDisconnected += reason => Disconnected?.Invoke(reason);
        channel.StartReadNotifications<TNotification>(OnNotification);
    }

    protected abstract void OnNotification(TNotification notification);

    protected async Task<TWanted> GetAsync<TWanted>(TRequest request, CancellationToken cancellationToken = default) where TWanted : TResponse
    {
        var result = await Channel.SendAsync<TRequest, TResponse>(request, cancellationToken);
        return result switch
        {
            null => throw new Exception("Result is null. Wat"),
            FailureResponse r => throw new Exception(r.Message),
            TWanted r => r,
            _ => throw new Exception($"Unknown result '{result.GetType().Name}'")
        };
    }

    protected Task<TResponse> SendAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        return Channel.SendAsync<TRequest, TResponse>(request, cancellationToken);
    }

    public async Task DisconnectAsync()
    {
        await Channel.DisconnectAsync();
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