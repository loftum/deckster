namespace Deckster.Server.Games.CrazyEights;

public class Locked<T>
{
    private readonly object _lock = new();
    private T? _item;

    public T? Item
    {
        get
        {
            lock (_lock)
            {
                return _item;    
            }
            
        }
        set
        {
            lock (_lock)
            {
                _item = value;    
            }
        }
    }

    public bool TryGet(out T value)
    {
        lock (_lock)
        {
            value = _item;
            if (value != null)
            {
                _item = default;
            }
        }
        return value != null;
    }

    public bool TrySet(T value)
    {
        lock (_lock)
        {
            if (_item != null)
            {
                return false;
            }
            _item = value;
            return true;
        }
    }
}