namespace Deckster.Communication.Handshake;

public class PlayerData : ServerHelloMessage
{
    public string Name { get; init; }
    public Guid PlayerId { get; init; }
}