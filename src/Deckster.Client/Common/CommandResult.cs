namespace Deckster.Client.Common;

public abstract class CommandResult : IHaveDiscriminator
{
    public string Type => GetType().Name;
}