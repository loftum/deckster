using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Client.Logging;

namespace Deckster.Server.Games.CrazyEights;

public class InProcessDecksterChannel : IDecksterChannel
{
    public InProcessDecksterChannel Target { get; }
    
    public PlayerData PlayerData { get; }
    public event Action<IDecksterChannel, byte[]>? OnMessage;
    public event Func<IDecksterChannel, Task>? OnDisconnected;

    private readonly Synchronizer _synchronizer = new();
    private readonly Locked<object> _response = new();

    private readonly ILogger _logger;

    public InProcessDecksterChannel(PlayerData playerData)
    {
        PlayerData = playerData;
        Target = new InProcessDecksterChannel(playerData, this);
        _logger = Log.Factory.CreateLogger($"{playerData.Name} (client)");
    }

    private InProcessDecksterChannel(PlayerData playerData, InProcessDecksterChannel target)
    {
        PlayerData = playerData;
        Target = target;
        _logger = Log.Factory.CreateLogger($"{playerData.Name} (target)");
    }
    
    public Task DisconnectAsync()
    {
        var handler = Target.OnDisconnected;
        return handler != null ? handler.Invoke(this) : Task.CompletedTask;
    }

    public async Task SendAsync<TRequest>(TRequest message, CancellationToken cancellationToken = default)
    {
        var handler = Target.OnMessage;
        if (handler == null)
        {
            return;
        }

        using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, message, DecksterJson.Options, cancellationToken);
        handler.Invoke(Target, memoryStream.ToArray());
    }

    public async Task<T?> ReceiveAsync<T>(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Await {val}", typeof(T).Name);
        
        var val = await _synchronizer;

        _logger.LogInformation("Done await {val}", val.GetType().Name);
        if (val is T t)
        {
            return t;
        }

        return default;
    }

    public async Task RespondAsync<TResponse>(TResponse response, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Responding {val}", response.GetType().Name);
        Target._synchronizer.SetResult(response);
        _logger.LogTrace("Done Responding {val}", response.GetType().Name);
    }

    public void Dispose()
    {
        
    }
}

internal class Synchronizer
{
    private readonly object _lock = new();
    private readonly Queue<SynchronizerAwaiter> _awaiters = new();

    public SynchronizerAwaiter GetAwaiter()
    {
        lock (_lock)
        {
            if (_awaiters.TryDequeue(out var awaiter))
            {
                return awaiter;
            }
            awaiter = new SynchronizerAwaiter();
            _awaiters.Enqueue(awaiter);
            return awaiter;
        }
    }

    public void SetResult(object value)
    {
        SynchronizerAwaiter awaiter;
        lock (_lock)
        {
            if (_awaiters.TryDequeue(out var a))
            {
                awaiter = a;
            }
            else
            {
                awaiter = new SynchronizerAwaiter();
                _awaiters.Enqueue(awaiter);
            }
        }
        awaiter.Result = value;
    }
}

internal class SynchronizerAwaiter : INotifyCompletion
{
    private Action? _continuation;
    private object? _result;
    

    public object? Result
    {
        get => _result;
        set
        {
            Console.WriteLine("Set result");
            Console.WriteLine($"Continuation == null? {_continuation == null}");
            _result = value;
            IsCompleted = true;
            if (_continuation != null)
            {
                Console.WriteLine("Invoking onComplete");
                _continuation.Invoke();
            }
        }
    }

    public bool IsCompleted { get; private set; }
    
    public object? GetResult()
    {
        Console.WriteLine("GET RESULT");
        return Result;
    }

    public void OnCompleted(Action continuation)
    {
        if (IsCompleted)
        {
            Console.WriteLine("Invoking onCompleted");
            continuation.Invoke();
        }
        else
        {
            Console.WriteLine("Setting onCompleted");
            _continuation = continuation;    
        }
    }
}