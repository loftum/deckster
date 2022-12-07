namespace Deckster.Core.Games.CrazyEights.Commands;

public class Pass : ICommand<CrazyEightsGame>
{
    public Guid PlayerId { get; init; }
    
    public ICommandResult ExecuteAsync(CrazyEightsGame game)
    {
        return game.Pass(PlayerId);
    }
}