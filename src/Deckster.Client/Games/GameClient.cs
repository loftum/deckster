using Deckster.Client.Common;
using Deckster.Client.Communication;

namespace Deckster.Client.Games;

public abstract class GameClient<TRequest, TResponse, TNotification> : IDisposable, IAsyncDisposable
{
    protected readonly IClientChannel<TRequest, TResponse, TNotification> Channel;
    public event Action? Disconnected;

    protected GameClient(IClientChannel<TRequest, TResponse, TNotification> channel)
    {
        Channel = channel;
        channel.OnDisconnected += (reason) => Disconnected?.Invoke();
    }

    protected async Task<TWanted> GetAsync<TWanted>(TRequest request, CancellationToken cancellationToken = default) where TWanted : TResponse
    {
        var result = await Channel.SendAsync(request, cancellationToken);
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
        return Channel.SendAsync(request, cancellationToken);
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