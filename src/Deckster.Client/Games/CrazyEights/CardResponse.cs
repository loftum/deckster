using Deckster.Client.Games.Common;

namespace Deckster.Client.Games.CrazyEights;

public class CrazyEightsFailureResponse : CrazyEightsResponse
{
    public string Message { get; init; }

    public CrazyEightsFailureResponse()
    {
        
    }
    
    public CrazyEightsFailureResponse(string message)
    {
        Message = message;
    }
}

public class CrazyEightsSuccessResponse : CrazyEightsResponse
{
    
}
    

public class CardResponse : CrazyEightsResponse
{
    public Card Card { get; init; }

    public CardResponse()
    {
        
    }

    public CardResponse(Card card)
    {
        Card = card;
    }
}