using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Client.Communication.WebSockets;
using Deckster.Server.Communication;
using Deckster.Server.Data;
using Deckster.Server.Games.CrazyEights;

namespace Deckster.Server.Games;

public class GameHostRegistry
{
    private readonly ConcurrentDictionary<Guid, ConnectingPlayer> _connectingPlayers = new();
    private readonly ConcurrentDictionary<string, IGameHost> _hostedGames = new();

    public GameHostRegistry(IHostApplicationLifetime lifetime)
    {
        lifetime.ApplicationStopping.Register(ApplicationStopping);
    }

    public void Add(IGameHost host)
    {
        _hostedGames.TryAdd(host.Name, host);
    }

    private void RemoveHost(object? sender, IGameHost e)
    {
        _hostedGames.TryRemove(e.Name, out _);
    }
    
    public IEnumerable<TGameHost> GetHosts<TGameHost>() where TGameHost : IGameHost
    {
        return _hostedGames.Values.OfType<TGameHost>();
    }

    public bool TryGet(string id, [MaybeNullWhen(false)] out IGameHost o)
    {
        return _hostedGames.TryGetValue(id, out o);
    }

    public async Task<bool> StartJoinAsync(DecksterUser user, WebSocket actionSocket, string gameHostName)
    {
        if (!_hostedGames.TryGetValue(gameHostName, out var host))
        {
            await actionSocket.SendMessageAsync(new ConnectFailureMessage
            {
                ErrorMessage = $"Unknown game '{gameHostName}'" 
            });
            return false;
        }

        var player = new PlayerData
        {
            Name = user.Name,
            Id = user.Id
        };
        var connectingPlayer = new ConnectingPlayer(player, actionSocket, host);
        if (!_connectingPlayers.TryAdd(connectingPlayer.ConnectionId, connectingPlayer))
        {
            await actionSocket.SendMessageAsync(new ConnectFailureMessage
            {
                ErrorMessage = "ConnectionId conflict"
            });
            return false;
        }

        await actionSocket.SendMessageAsync(new HelloSuccessMessage
        {
            ConnectionId = connectingPlayer.ConnectionId,
            Player = connectingPlayer.Player
        });

        await connectingPlayer.TaskCompletionSource.Task;
        return true;
    }
    
    public async Task<bool> FinishJoinAsync(Guid connectionId, WebSocket eventSocket)
    {
        if (!_connectingPlayers.TryRemove(connectionId, out var connecting))
        {
            await eventSocket.SendMessageAsync<ConnectMessage>(new ConnectFailureMessage
            {
                ErrorMessage = $"Invalid connectionId: '{connectionId}'"
            });
            return false;
        }
        
        var channel = new WebSocketServerChannel(connecting.Player, connecting.ActionSocket, eventSocket, connecting.TaskCompletionSource);
        if (!connecting.GameHost.TryAddPlayer(channel, out var error))
        {
            await eventSocket.SendMessageAsync<ConnectMessage>(new ConnectFailureMessage
            {
                ErrorMessage = error
            });
            await channel.DisconnectAsync();
            channel.Dispose();
            return false;
        }
        
        await eventSocket.SendMessageAsync<ConnectMessage>(new ConnectSuccessMessage());
        await connecting.TaskCompletionSource.Task;
        return true;
    }
    
    private async void ApplicationStopping()
    {
        foreach (var connecting in _connectingPlayers.Values.ToArray())
        {
            await connecting.CancelAsync();
        }

        foreach (var host in _hostedGames.Values.ToArray())
        {
            await host.CancelAsync();
        }
    }
}