namespace Deckster.Core.Games.CrazyEights;

public class FailureResult : ICommandResult
{
    public string Message { get; }
    
    public FailureResult(string message)
    {
        Message = message;
    }
}