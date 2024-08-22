using Deckster.Client.Protocol;

namespace Deckster.Client.Common;

public class FailureResult : DecksterCommandResult
{
    public string Message { get; }
    
    public FailureResult(string message)
    {
        Message = message;
    }
}