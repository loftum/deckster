using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using Deckster.Server.Data;
using Deckster.Server.Games.CrazyEights;

namespace Deckster.Server.Games;

public class GameRegistry
{
    private readonly ConcurrentDictionary<Guid, ConnectingPlayer> _connectingPlayers = new();
    
    private readonly ConcurrentDictionary<Guid, IGameHost> _hostedGames = new();

    public void Add(IGameHost host)
    {
        _hostedGames.TryAdd(host.Id, host);
        host.OnEnded += RemoveHost;
    }

    private void RemoveHost(object? sender, IGameHost e)
    {
        e.OnEnded -= RemoveHost;
        _hostedGames.TryRemove(e.Id, out _);
    }

    public bool TryGet(Guid id, out IGameHost o)
    {
        return _hostedGames.TryGetValue(id, out o);
    }

    public bool TryStartConnect(DecksterUser user, WebSocket commandSocket, Guid gameId, [MaybeNullWhen(false)] out ConnectingPlayer connectingPlayer)
    {
        connectingPlayer = default;
        if (!_hostedGames.TryGetValue(gameId, out var host))
        {
            return false;
        }

        connectingPlayer = new ConnectingPlayer(user, commandSocket, host);
        return true;
    }
    
    public async Task<bool> TryCompleteAsync(Guid connectionId, WebSocket eventSocket)
    {
        if (!_connectingPlayers.TryRemove(connectionId, out var connectingUser))
        {
            return false;
        }
        
        var channel = new ServerChannel(connectingUser.User, connectingUser.CommandSocket, eventSocket);
        if (!connectingUser.GameHost.TryAddPlayer(channel, out var error))
        {
            await channel.DisconnectAsync();
            channel.Dispose();
            return false;
        }
        
        return true;
    }
}