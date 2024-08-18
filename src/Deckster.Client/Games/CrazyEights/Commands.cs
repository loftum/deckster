using Deckster.Client.Communication;
using Deckster.Client.Games.Common;

namespace Deckster.Client.Games.CrazyEights;

public class PutCardCommand : DecksterCommand
{
    public Card Card { get; set; }
}

public class PutEightCommand : DecksterCommand
{
    public Card Card { get; set; }
    public Suit NewSuit { get; set; }
}

public class DrawCardCommand : DecksterCommand
{
    
}

public class PassCommand : DecksterCommand
{
    
}