namespace Deckster.Client.Protocol;

public interface IHaveDiscriminator
{
    // ReSharper disable once UnusedMemberInSuper.Global
    string Type { get; }
}