using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Client.Games.Common;

namespace Deckster.Client.Games.CrazyEights;

[JsonDerived<DecksterCommand>]
public abstract class DecksterCommand : IHaveDiscriminator
{
    public string Type => GetType().Name.Replace("Command", "");
}

public class StartCommand : DecksterCommand
{
    
}

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