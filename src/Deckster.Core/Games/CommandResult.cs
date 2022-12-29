namespace Deckster.Core.Games;

public abstract class CommandResult : IHaveDiscriminator
{
    public string Discriminator => GetType().Name;
}