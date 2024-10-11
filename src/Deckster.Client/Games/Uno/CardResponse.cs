namespace Deckster.Client.Games.Uno;

public class UnoCardResponse : UnoResponse
{
    public UnoCard Card { get; init; }

    public UnoCardResponse()
    {
        
    }

    public UnoCardResponse(UnoCard card)
    {
        Card = card;
    }
}