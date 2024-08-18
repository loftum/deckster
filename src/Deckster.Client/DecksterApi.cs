using Deckster.Client.Games.ChatRoom;
using Deckster.Client.Games.CrazyEights;

namespace Deckster.Client;

public class DecksterApi
{
    public GameApi<CrazyEightsClient> CrazyEights { get; }
    public GameApi<ChatRoomClient> ChatRoom { get; }

    public DecksterApi(Uri baseUri, string token)
    {
        Console.WriteLine($"Baseurl: {baseUri}, crazyeights: {baseUri.Append("crazyeights")}");
        CrazyEights = new GameApi<CrazyEightsClient>(baseUri.Append("crazyeights"), token, c => new CrazyEightsClient(c));
        ChatRoom = new GameApi<ChatRoomClient>(baseUri.Append("chatroom"), token, c => new ChatRoomClient(c));
    }
}