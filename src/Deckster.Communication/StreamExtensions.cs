using System.Text.Json;

namespace Deckster.Communication;

public static class StreamExtensions
{
    public static async Task SendMessageAsync(this Stream stream, byte[] message, CancellationToken cancellationToken = default)
    {
        await stream.WriteAsync(ToBytes(message.Length), cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public static byte[] ToBytes(this int length)
    {
        var bytes = new byte[8];
        bytes[3] = (byte) (length >> 24 & 0xff);
        bytes[2] = (byte) (length >> 16 & 0xff);
        bytes[1] = (byte) (length >> 8 & 0xff);
        bytes[0] = (byte) (length & 0xff);
        return bytes;
    }
    
    public static async Task<byte[]> ReceiveMessageAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        var lengthBytes = new byte[4];
        var read = await stream.ReadAsync(lengthBytes, 0, 4, cancellationToken);
        var length = ToInt(lengthBytes);
        var messageBytes = new byte[length];
        read = await stream.ReadAsync(messageBytes, cancellationToken);
        return messageBytes;
    }
    
    public static uint ToInt(this byte[] bytes)
    {
        return (uint)(bytes[0] |
                      bytes[1] << 8 |
                      bytes[2] << 16 |
                      bytes[3] << 24);
    }

    public static Task SendJsonAsync<T>(this Stream stream, T message, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        JsonSerializer.Serialize(memoryStream, message, options);
        return stream.SendMessageAsync(memoryStream.ToArray(), cancellationToken);
    }
    
    public static async Task<T?> ReceiveJsonAsync<T>(this Stream stream, JsonSerializerOptions options, CancellationToken cancellationToken = default)
    {
        var message = await stream.ReceiveMessageAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(message, options);
    }
}