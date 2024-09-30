using Deckster.Client.Common;
using Deckster.Server.Games.Common;

namespace Deckster.Server.Controllers;

public class GameVm
{
    public Guid Id { get; init; }
    public GameState State { get; init; }
    public ICollection<PlayerData> Players { get; init; } = [];
}