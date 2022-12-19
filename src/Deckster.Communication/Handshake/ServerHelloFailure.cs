namespace Deckster.Communication.Handshake;

public class ServerHelloFailure : ServerHelloMessage
{
    public string ErrorMessage { get; set; }
}