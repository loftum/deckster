using System.Net.WebSockets;
using Deckster.Client.Communication;
using Deckster.Client.Communication.WebSockets;
using Deckster.Server.Authentication;
using Deckster.Server.Games;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

public abstract class CardGameController : Controller
{
    protected readonly GameRegistry Registry;

    protected CardGameController(GameRegistry registry)
    {
        Registry = registry;
    }
    
    [HttpPost("start/{id:guid}")]
    public async Task<object> Start(Guid id)
    {
        if (!Registry.TryGet(id, out var host))
        {
            return StatusCode(404, new ResponseMessage("Game not found: '{id}'"));
        }
        
        await host.Start();
        return StatusCode(200, new ResponseMessage("Game '{id}' started"));
    }
    
    [HttpGet("join/{gameId:guid}")]
    public async Task Join(Guid gameId)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Not WS request"));
            return;
        }

        if (!HttpContext.TryGetUser(out var decksterUser))
        {
            HttpContext.Response.StatusCode = 401;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Unauthorized"));
            return;
        }
        using var actionSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        
        if (!await Registry.StartJoinAsync(decksterUser, actionSocket, gameId))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Could not connect"));
            await actionSocket.CloseOutputAsync(WebSocketCloseStatus.ProtocolError, "Could not connect", default);
        }
    }

    [HttpGet("join/{connectionId:guid}/finish")]
    public async Task FinishJoin(Guid connectionId)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Not WS request"));
            return;
        }
        
        using var eventSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        if (!await Registry.FinishJoinAsync(connectionId, eventSocket))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Could not connect"));
        }
    }
}