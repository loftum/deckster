namespace Deckster.Server.Infrastructure;

public interface IDecksterMiddleware
{
    Task InvokeAsync(DecksterContext context);
}