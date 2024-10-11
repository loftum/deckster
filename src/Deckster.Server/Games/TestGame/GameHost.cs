using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Common;
using Deckster.Server.Communication;
using Deckster.Server.Games.Common;

namespace Deckster.Server.Games.TestGame;

public abstract class GameHost<TRequest, TResponse, TNotification> : IGameHost
{
    public event EventHandler<IGameHost>? OnEnded;
    public abstract string GameType { get; }
    public string Name { get; init; }
    public abstract GameState State { get; }

    public abstract Task Start();

    public abstract bool TryAddPlayer(IServerChannel channel, [MaybeNullWhen(true)] out string error);
    public abstract Task CancelAsync();
    public abstract ICollection<PlayerData> GetPlayers();
}