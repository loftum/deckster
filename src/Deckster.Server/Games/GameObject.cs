using Deckster.Client.Protocol;
using Deckster.Server.Data;
using Deckster.Server.Games.Common;

namespace Deckster.Server.Games;

public delegate Task NotifyAll<in T>(T notification) where T : DecksterNotification;
public delegate Task NotifyPlayer<in T>(Guid playerId, T notification) where T : DecksterNotification;

public abstract class GameObject : DatabaseObject
{
    protected ICommunication Communication = NullCommunication.Instance;
    
    public Func<Guid, DecksterResponse, Task> RespondAsync { get; set; } = (_, _) => Task.CompletedTask;
    
    public DateTimeOffset StartedTime { get; init; }
    public abstract GameState State { get; }

    // ReSharper disable once UnusedMember.Global
    // Used by Marten
    public int Version { get; set; }

    public void SetCommunication(ICommunication communication)
    {
        Communication = communication;
    }

    public abstract Task StartAsync();
}
