using System.Net.WebSockets;
using Deckster.Server.Data;

namespace Deckster.Server.Games.CrazyEights;

public class ConnectingPlayer
{
    public TaskCompletionSource TaskCompletionSource { get; } = new();
    public Guid ConnectionId { get; } = Guid.NewGuid();
    public DecksterUser User { get; }
    public WebSocket CommandSocket { get; }
    public IGameHost GameHost { get; }
    
    public ConnectingPlayer(DecksterUser user, WebSocket commandSocket, IGameHost host)
    {
        User = user;
        CommandSocket = commandSocket;
        GameHost = host;
    }
}