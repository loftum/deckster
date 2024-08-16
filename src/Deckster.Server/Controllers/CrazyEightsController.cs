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
    private readonly GameRegistry _registry;

    public CrazyEightsController(IRepo repo, GameRegistry registry)
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
        var host = new CrazyEightsGameHost();
        _registry.Add(host);
        return StatusCode(200, new { host.Id });
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
}
