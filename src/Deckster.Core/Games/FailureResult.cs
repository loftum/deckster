namespace Deckster.Core.Games;

public class FailureResult : CommandResult
{
    public string Message { get; }
    
    public FailureResult(string message)
    {
        Message = message;
    }
}