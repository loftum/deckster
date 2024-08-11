using System.Collections.Concurrent;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsGameRegistry
{
    private readonly ConcurrentDictionary<Guid, CrazyEightsGameHost> _hostedGames = new();

    public CrazyEightsGameHost Create()
    {
        var host = new CrazyEightsGameHost();
        host.OnEnded += RemoveHost;
        return host;
    }

    private void RemoveHost(object? sender, CrazyEightsGameHost e)
    {
        e.OnEnded -= RemoveHost;
        _hostedGames.TryRemove(e.Id, out _);
    }


    public bool TryGet(Guid id, out CrazyEightsGameHost o)
    {
        return _hostedGames.TryGetValue(id, out o);
    }
}