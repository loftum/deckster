namespace Deckster.Communication.Handshake;

public class ConnectResponse
{
    public int StatusCode { get; set; } = 200;
    public string? Description { get; set; }
    public PlayerData PlayerData { get; init; }
}