namespace Deckster.Server.Games.Common.Fakes;

public class MessagePipe<TMessage>
{
    private readonly TaskCompletionSource<TMessage> _tcs = new();
    private readonly Queue<TMessage> _messges = new();
    
    public void Add(TMessage message)
    {
        _messges.Enqueue(message);
        _tcs.SetResult(message);
    }

    public Task<TMessage> ReadAsync()
    {
        return _tcs.Task;
    }
}