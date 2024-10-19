using System.Text.Json;
using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Client.Protocol;
using Deckster.Server.Communication;

namespace Deckster.Server.Games.Common.Fakes;

public partial class InMemoryChannel : IClientChannel
{
    public event Action<string>? OnDisconnected;
    private readonly MessagePipe<byte[]> _requests = new();
    private readonly MessagePipe<byte[]> _responses = new();
    private Task _readNotificationsTask;
    private readonly CancellationTokenSource _cts = new();
    
    public async Task<TResponse> SendAsync<TResponse>(object request, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        _requests.Add(JsonSerializer.SerializeToUtf8Bytes(request, options));
        var bytes = await _responses.ReadAsync();
        var response = JsonSerializer.Deserialize<TResponse>(bytes, options);
        if (response == null)
        {
            throw new Exception("OMG GOT NULLZ RESULTZ!");
        }

        return response;
    }

    public void StartReadNotifications<TNotification>(Action<TNotification> handle, JsonSerializerOptions options)
    {
        _readNotificationsTask = ReadNotificationsAsync(handle, options);
    }

    private async Task ReadNotificationsAsync<TNotification>(Action<TNotification> handle, JsonSerializerOptions options)
    {
        while (!_cts.IsCancellationRequested)
        {
            var bytes = await _notifications.ReadAsync();
            var notification = JsonSerializer.Deserialize<TNotification>(bytes, options);
            if (notification == null)
            {
                throw new Exception("OMG GOT NULLZ NOETFICATION!");
            }   
            handle(notification);
        }
    }
    
    public ValueTask DisposeAsync()
    {
        _cts.Dispose();
        return ValueTask.CompletedTask;
    }
}

public partial class InMemoryChannel : IServerChannel
{
    public event Action<IServerChannel>? Disconnected;
    public PlayerData Player { get; init; }

    private Task _readRequestsTask;

    private readonly MessagePipe<byte[]> _notifications = new();

    public void Start<TRequest>(Action<IServerChannel, TRequest> handle, JsonSerializerOptions options, CancellationToken cancellationToken) where TRequest : DecksterRequest
    {
        _readRequestsTask = ReadRequestsAsync(handle, options, cancellationToken);
    }

    private async Task ReadRequestsAsync<TRequest>(Action<IServerChannel, TRequest> handle, JsonSerializerOptions options, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var bytes = await _requests.ReadAsync();
            var request = JsonSerializer.Deserialize<TRequest>(bytes, options);
            if (request == null)
            {
                throw new Exception("OMG GOT NULLZ REKWEST!");
            }

            handle(this, request);
        }
    }
    
    public ValueTask ReplyAsync<TResponse>(TResponse response, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        _responses.Add(JsonSerializer.SerializeToUtf8Bytes(response, options));
        return ValueTask.CompletedTask;
    }

    public ValueTask SendNotificationAsync<TNotification>(TNotification notification, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        _notifications.Add(JsonSerializer.SerializeToUtf8Bytes(notification, options));
        return ValueTask.CompletedTask;
    }

    public Task WeAreDoneHereAsync(CancellationToken cancellationToken = default)
    {
        return DisconnectAsync();
    }

    public Task DisconnectAsync()
    {
        OnDisconnected?.Invoke("hest");
        Disconnected?.Invoke(this);
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        
    }
}