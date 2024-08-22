using Deckster.Client.Communication;

namespace Deckster.Client.Protocol;

[JsonDerived<DecksterCommand>]
public abstract class DecksterCommand : IHaveDiscriminator
{
    public string Type => GetType().Name.Replace("Command", "");
}

