using System.Net.WebSockets;
using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Server.Authentication;
using Deckster.Server.Games;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

public abstract class CardGameController : Controller
{
    private readonly GameRegistry _gameRegistry;

    protected CardGameController(GameRegistry gameRegistry)
    {
        _gameRegistry = gameRegistry;
    }
    
    [HttpPost("join/{gameId:guid}")]
    public async Task Join(Guid gameId)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Not WS request"));
            return;
        }

        var decksterUser = HttpContext.GetRequiredUser();
        var commandSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        if (!_gameRegistry.TryStartConnect(decksterUser, commandSocket, gameId, out var connectingPlayer))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Could not connect"));
            await commandSocket.CloseOutputAsync(WebSocketCloseStatus.ProtocolError, "Could not connect", default);
            commandSocket.Dispose();
            return;
        }
        
        await connectingPlayer.CommandSocket.SendMessageAsync(new ConnectMessage
        {
            FinishUri = new Uri(HttpContext.Request.),
            PlayerData = new PlayerData
            {
                Name = decksterUser.Name,
                PlayerId = decksterUser.Id
            }
        });
    }

    [HttpPost("join/finish/{connectionId:guid}")]
    public async Task FinishJoin(Guid connectionId)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Not WS request"));
            return;
        }
        
        var eventSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        if (!await _gameRegistry.TryCompleteAsync(connectionId, eventSocket))
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new ResponseMessage("Could not connect"));
        }
    }
}
