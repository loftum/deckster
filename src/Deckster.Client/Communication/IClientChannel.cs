using Deckster.Client.Common;

namespace Deckster.Client.Communication;

public interface IClientChannel<in TRequest, TResponse, out TNotification> : IDisposable, IAsyncDisposable
{
    PlayerData PlayerData { get; }
    event Action<TNotification>? OnMessage;
    event Action<string>? OnDisconnected;
    Task DisconnectAsync();
    Task<TResponse> SendAsync(TRequest request, CancellationToken cancellationToken = default);
}

public static class ClientChannelExtensions
{
    public static async Task<TWanted> GetAsync<TRequest, TResponse, TNotification, TWanted>(this IClientChannel<TRequest, TResponse, TNotification> channel, TRequest request,
        CancellationToken cancellationToken = default)
        where TWanted : TResponse
    {
        var result = await channel.SendAsync(request, cancellationToken);
        return result switch
        {
            null => throw new Exception("Result is null. Wat"),
            FailureResponse r => throw new Exception(r.Message),
            TResponse r => r,
            _ => throw new Exception($"Unknown result '{result.GetType().Name}'")
        };
    }
}