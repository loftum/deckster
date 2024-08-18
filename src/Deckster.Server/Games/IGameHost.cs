using System.Diagnostics.CodeAnalysis;
using Deckster.Server.Games.CrazyEights;

namespace Deckster.Server.Games;

public interface IGameHost
{
    event EventHandler<CrazyEightsGameHost> OnEnded;
    Guid Id { get; }
    Task Start();
    bool TryAddPlayer(ServerChannel player, [MaybeNullWhen(true)] out string error);
}