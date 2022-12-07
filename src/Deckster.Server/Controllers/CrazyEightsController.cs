using Deckster.Core.Domain;
using Deckster.Core.Games.CrazyEights;
using Deckster.Server.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

[Route("crazyeights")]
[RequireUser]
public class CrazyEightsController : Controller
{
    private readonly CrazyEightsRepo _repo;
    private readonly User _user;

    public CrazyEightsController(CrazyEightsRepo repo)
    {
        _repo = repo;
        _user = HttpContext.GetRequiredUser();
    }

    [HttpPut("{gameId}/pile")]
    public async Task<object> PutCardOnPile(Guid gameId, [FromBody] Card card)
    {
        var game = await _repo.GetAsync(gameId);
        if (game == null)
        {
            return NotFound(new FailureResult($"There is no game '{gameId}'"));
        }

        return game.PutCardOnDiscardPile(_user.Id, card);
    }

    [HttpGet("state")]
    public async Task<object> GetState(Guid gameId)
    {
        var game = await _repo.GetAsync(gameId);
        if (game == null)
        {
            return NotFound(new FailureResult($"There is no game '{gameId}'"));
        }

        var state = game.GetStateFor(_user.Id);
        return CommandResult(state);
    }

    private object CommandResult(ICommandResult result)
    {
        return result switch
        {
            SuccessResult r => Ok(r),
            FailureResult r => StatusCode(400, r),
            _ => StatusCode(500, result)
        };
    }
}