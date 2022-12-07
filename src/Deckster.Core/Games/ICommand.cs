using Deckster.Core.Games.CrazyEights;

namespace Deckster.Core.Games;

public interface ICommand<in TGame>
{
    ICommandResult ExecuteAsync(TGame game);
}