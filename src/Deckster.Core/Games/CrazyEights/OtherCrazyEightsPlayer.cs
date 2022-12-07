namespace Deckster.Core.Games.CrazyEights;

public class OtherCrazyEightsPlayer
{
    public string Name { get; init; }
    public int NumberOfCards { get; init; }
    public bool IsDone => NumberOfCards == 0;
}