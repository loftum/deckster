using Deckster.Client.Protocol;
using Deckster.Client.Serialization;

namespace Deckster.Client.Games.ChatRoom;

[JsonDerived<ChatRequest>]
public abstract class ChatRequest : IHaveDiscriminator
{
    public string Type => GetType().Name;
}

public class SendChatMessage : ChatRequest
{
    public string Message { get; set; }
}

[JsonDerived<ChatNotification>]
public class ChatNotification : IHaveDiscriminator
{
    public string Type => GetType().Name;
    public string Sender { get; init; }
    public string Message { get; init; }
}

[JsonDerived<ChatResponse>]
public class ChatResponse : IHaveDiscriminator
{
    public string Type => GetType().Name;
}