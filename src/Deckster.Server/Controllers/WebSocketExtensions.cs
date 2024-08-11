using System.Net.WebSockets;
using System.Text.Json;
using Deckster.Server.Serialization;

namespace Deckster.Server.Controllers;

public static class WebSocketExtensions
{
    public static ValueTask SendMessageAsync(this WebSocket socket, WebSocketMessage message, CancellationToken cancellationToken = default)
    {
        return socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(message, Jsons.CamelCase),
            WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, cancellationToken);
    }
}