namespace Deckster.Core.Games;

public interface ICommand<in TGame>
{
    CommandResult ExecuteAsync(TGame game);
}