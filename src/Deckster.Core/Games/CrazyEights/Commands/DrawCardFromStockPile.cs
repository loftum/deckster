namespace Deckster.Core.Games.CrazyEights.Commands;

public class DrawCardFromStockPile : ICommand<CrazyEightsGame>
{
    public Guid PlayerId { get; init; }
    
    public ICommandResult ExecuteAsync(CrazyEightsGame game)
    {
        return game.DrawCardFromStockPile(PlayerId);
    }
}