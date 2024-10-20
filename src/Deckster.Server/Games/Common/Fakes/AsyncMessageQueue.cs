namespace Deckster.Server.Games.Common.Fakes;

public class AsyncMessageQueue<TMessage>
{
    private TaskCompletionSource<TMessage> _tcs = new();
    private readonly Queue<TMessage> _messages = new();

    private readonly object _lock = new();
    
    public void Add(TMessage message)
    {
        lock (_lock)
        {
            _messages.Enqueue(message);
            TryDequeueMessage();
        }
    }

    private void TryDequeueMessage()
    {
        if (!_tcs.Task.IsCompleted && _messages.TryDequeue(out var m))
        {
            _tcs.SetResult(m);
        }
    }

    public Task<TMessage> ReadAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            var current = _tcs;
            if (current.Task.IsCompleted)
            {
                _tcs = new TaskCompletionSource<TMessage>();
                TryDequeueMessage();
            }
            cancellationToken.Register(() => current.SetCanceled(cancellationToken));
            return current.Task;
        }
    }
}