using Deckster.Client.Communication;

namespace Deckster.Client.Protocol;

[JsonDerived<DecksterCommandResult>]
public abstract class DecksterCommandResult : IHaveDiscriminator
{
    public string Type => GetType().Name;
}