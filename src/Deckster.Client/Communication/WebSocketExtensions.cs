using System.Net.WebSockets;
using System.Text.Json;
using Deckster.Client.Serialization;

namespace Deckster.Client.Communication;

public static class WebSocketExtensions
{
    public static ValueTask SendMessageAsync<T>(this WebSocket socket, T message, CancellationToken cancellationToken = default)
    {
        return socket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(message, Jsons.CamelCase),
            WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, cancellationToken);
    }

    public static async Task<T?> ReceiveMessageAsync<T>(this WebSocket socket, CancellationToken cancellationToken = default)
    {
        var buffer = new byte[512];
        var result = await socket.ReceiveAsync(buffer, cancellationToken);
        
        switch (result.MessageType)
        {
            case WebSocketMessageType.Text:
                return JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(buffer, 0, result.Count));
            case WebSocketMessageType.Close:
                return default;
        }

        return default;
    }
}