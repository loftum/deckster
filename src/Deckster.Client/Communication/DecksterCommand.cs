using Deckster.Client.Common;

namespace Deckster.Client.Communication;

[JsonDerived<DecksterCommand>]
public abstract class DecksterCommand : IHaveDiscriminator
{
    public string Type => GetType().Name.Replace("Command", "");
}

