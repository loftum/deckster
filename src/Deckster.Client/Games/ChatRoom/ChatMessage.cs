using Deckster.Client.Communication;
using Deckster.Client.Games.CrazyEights;

namespace Deckster.Client.Games.ChatRoom;

public class ChatMessage : DecksterCommand
{
    public string Message { get; set; }
}