using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using Deckster.Client.Common;
using Deckster.Client.Communication;
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

    public async Task<bool> StartJoinAsync(DecksterUser user, WebSocket commandSocket, Guid gameId)
    {
        if (!_hostedGames.TryGetValue(gameId, out var host))
        {
            return false;
        }

        var connectingPlayer = new ConnectingPlayer(user, commandSocket, host);
        if (!_connectingPlayers.TryAdd(connectingPlayer.ConnectionId, connectingPlayer))
        {
            return false;
        }

        await commandSocket.SendMessageAsync(new ConnectMessage
        {
            ConnectionId = connectingPlayer.ConnectionId,
            PlayerData = new PlayerData
            {
                Name = user.Name,
                PlayerId = user.Id
            }
        });

        await connectingPlayer.TaskCompletionSource.Task;
        return true;
    }
    
    public async Task<bool> FinishJoinAsync(Guid connectionId, WebSocket eventSocket)
    {
        if (!_connectingPlayers.TryRemove(connectionId, out var connectingUser))
        {
            return false;
        }
        
        var channel = new ServerChannel(connectingUser.User, connectingUser.CommandSocket, eventSocket, connectingUser.TaskCompletionSource);
        if (!connectingUser.GameHost.TryAddPlayer(channel, out var error))
        {
            await channel.DisconnectAsync();
            channel.Dispose();
            return false;
        }

        await connectingUser.TaskCompletionSource.Task;
        return true;
    }
}