using Deckster.Core.Domain;

namespace Deckster.Core.Games.CrazyEights.Commands;

public class PutCardOnDiscardPile : ICommand<CrazyEightsGame>
{
    public Guid PlayerId { get; init; }
    public Card Card { get; init; }
    
    public ICommandResult ExecuteAsync(CrazyEightsGame game)
    {
        return game.PutCardOnDiscardPile(PlayerId, Card);
    }
}