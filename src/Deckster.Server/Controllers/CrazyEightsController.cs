using System.Net.WebSockets;
using Deckster.Client.Common;
using Deckster.Server.Authentication;
using Deckster.Server.Data;
using Deckster.Server.Games;
using Deckster.Server.Games.CrazyEights;
using Deckster.Server.Games.CrazyEights.Core;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

[Route("crazyeights")]
[RequireUser]
public class CrazyEightsController : CardGameController
{
    private readonly IRepo _repo;
    private DecksterUser DecksterUser => HttpContext.GetRequiredUser();
    private readonly CrazyEightsGameRegistry _registry;

    public CrazyEightsController(IRepo repo, CrazyEightsGameRegistry registry)
    {
        _repo = repo;
        _registry = registry;
    }

    [HttpGet("")]
    public ViewResult Index()
    {
        return View();
    }

    [HttpPost("create")]
    public async Task<object> Create()
    {
        var host = _registry.Create();
        return StatusCode(200, new {host.Id});
    }

    [HttpPost("join/{id:guid}")]
    public async Task Join(Guid id)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Not WS request"));
            return;
        }

        if (!_registry.TryGet(id, out var host))
        {
            HttpContext.Response.StatusCode = 404;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Game not found: '{id}'"));
            return;
        }
        
        var websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        if (!host.TryAddPlayer(DecksterUser, websocket, out var reason))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage($"Could not add player: {reason}"));
            await websocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, reason, default);
            websocket.Dispose();
        }
    }

    [HttpPost("start/{id}")]
    public async Task<object> Start(Guid id)
    {
        if (!_registry.TryGet(id, out var host))
        {
            return StatusCode(404, new ResponseMessage("Game not found: '{id}'"));
        }
        
        await host.Start();
        return StatusCode(200, new ResponseMessage("Game '{id}' started"));
    }
    

    [HttpGet("{gameId}/state")]
    public async Task<object> GetState(Guid gameId)
    {
        var game = await _repo.GetAsync<CrazyEightsGame>(gameId);
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
