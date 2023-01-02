namespace Deckster.Client.Core.Games;

internal interface IHaveDiscriminator
{
    // ReSharper disable once UnusedMemberInSuper.Global
    string Discriminator { get; }
}