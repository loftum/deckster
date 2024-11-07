using Deckster.Core.Games.Common;
using Deckster.Core.Protocol;

namespace Deckster.Core.Games.Gabong;

public abstract class GabongResponse : DecksterResponse
{
    public List<Card> Punishment { get; init; } = [];
}
    
public class GabongCardResponse : GabongResponse
{
    
    public Card Card { get; init; }
}

public class ActionResponse : DecksterResponse
{
    
}