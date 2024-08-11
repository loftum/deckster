using Deckster.Client.Common;
using Deckster.Server.Authentication;
using Deckster.Server.Games.CrazyEights;
using Deckster.Server.Users;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

public abstract class CardGameController : Controller
{
    
}

[Route("crazyeights")]
[RequireUser]
public class CrazyEightsController : CardGameController
{
    private readonly CrazyEightsRepo _repo;
    private DecksterUser DecksterUser => HttpContext.GetRequiredUser();

    public CrazyEightsController(CrazyEightsRepo repo)
    {
        _repo = repo;
    }

    [HttpGet("")]
    public ViewResult Index() => View();

    [HttpGet("{gameId}/state")]
    public async Task<object> GetState(Guid gameId)
    {
        var game = await _repo.GetAsync(gameId);
        if (game == null)
        {
            return NotFound(new FailureResult($"There is no game '{gameId}'"));
        }

        var state = game.GetStateFor(DecksterUser.Id);
        return CommandResult(state);
    }

    private object CommandResult(CommandResult result)
    {
        return result switch
        {
            SuccessResult r => Ok(r),
            FailureResult r => StatusCode(400, r),
            _ => StatusCode(500, result)
        };
    }
}