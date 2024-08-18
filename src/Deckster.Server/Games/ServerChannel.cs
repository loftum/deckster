using System.Net.WebSockets;
using System.Text;
using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Server.Data;

namespace Deckster.Server.Games;

public class ServerChannel : IDisposable
{
    public event Action<Guid, DecksterCommand>? Received;
    
    public DecksterUser User { get; }
    private readonly WebSocket _commandSocket;
    private readonly WebSocket _eventSocket;
    private readonly TaskCompletionSource _taskCompletionSource;

    private Task? _listenTask;
    
    public ServerChannel(DecksterUser user, WebSocket commandSocket, WebSocket eventSocket, TaskCompletionSource taskCompletionSource)
    {
        User = user;
        _commandSocket = commandSocket;
        _eventSocket = eventSocket;
        _taskCompletionSource = taskCompletionSource;
    }

    public void Start(CancellationToken cancellationToken)
    {
        _listenTask = ListenAsync(cancellationToken);
    }
    
    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        try
        {
            var buffer = new byte[4096];
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await _commandSocket.ReceiveAsync(buffer, cancellationToken);
                Console.WriteLine($"Got messageType: '{result.MessageType}'");

                switch (result.MessageType)
                {
                    case WebSocketMessageType.Close:
                        _taskCompletionSource.SetResult();
                        await _commandSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Server closed connection", default);
                        return;
                }
            
                var command = DecksterJson.Deserialize<DecksterCommand>(new ArraySegment<byte>(buffer, 0, result.Count));
                if (command == null)
                {
                    Console.WriteLine("Command is null.");
                    Console.WriteLine($"Raw: {Encoding.UTF8.GetString(new ArraySegment<byte>(buffer, 0, result.Count))}");
                    await _commandSocket.SendMessageAsync(new FailureResult("Command is null"), cancellationToken: cancellationToken);
                }
                else
                {
                    Console.WriteLine($"Got command: {command.Pretty()}");
                    Received?.Invoke(User.Id, command);    
                }
            }
        }
        catch (TaskCanceledException e)
        {
            return;
        }
    }
    
    public ValueTask ReplayAsync(object message, CancellationToken cancellationToken = default)
    {
        var bytes = DecksterJson.SerializeToBytes(message);
        return _commandSocket.SendAsync(bytes, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, cancellationToken);
    }

    public ValueTask PostEventAsync(object message, CancellationToken cancellationToken = default)
    {
        var bytes = DecksterJson.SerializeToBytes(message);
        return _eventSocket.SendAsync(bytes, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, cancellationToken);
    }
    
    public Task WeAreDoneHereAsync(CancellationToken cancellationToken = default)
    {
        return _commandSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
    }

    public async Task DisconnectAsync()
    {
        await _commandSocket.CloseOutputAsync(WebSocketCloseStatus.ProtocolError, "Closing", default);
        await _eventSocket.CloseOutputAsync(WebSocketCloseStatus.ProtocolError, "Closing", default);
    }

    public void Dispose()
    {
        _commandSocket.Dispose();
        _eventSocket.Dispose();
        _taskCompletionSource.TrySetResult();
    }
}