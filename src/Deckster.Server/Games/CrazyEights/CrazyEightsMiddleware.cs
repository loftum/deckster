using Deckster.CrazyEights.Game;
using Deckster.Server.Infrastructure;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsMiddleware : IDecksterMiddleware
{
    private readonly DecksterDelegate _next;
    private readonly CrazyEightsRepo _gameRepo;
    private readonly ILogger<CrazyEightsMiddleware> _logger;

    public CrazyEightsMiddleware(CrazyEightsRepo gameRepo, ILogger<CrazyEightsMiddleware> logger, DecksterDelegate next)
    {
        _gameRepo = gameRepo;
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(ConnectionContext context)
    {
        
        context.Response.StatusCode = 404;
        context.Response.Description = "Could not find any games";
        return Task.CompletedTask;
    }
}

public class CrazyEightsGame
{
    
}