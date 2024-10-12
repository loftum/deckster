using Deckster.Client.Protocol;
using Deckster.Client.Serialization;

namespace Deckster.Client.Games.Uno;

[JsonDerived<UnoResponse>]
public abstract class UnoResponse : IHaveDiscriminator
{
    public string Type => GetType().Name;
}

public class UnoCardsResponse : UnoResponse
{
    public UnoCard Card { get; init; }

    public UnoCardsResponse()
    {
        
    }

    public UnoCardsResponse(UnoCard card)
    {
        Card = card;
    }
}