using Deckster.Core.Games.ChatRoom;
using System.Diagnostics;
using Deckster.Client.Communication;
using Deckster.Core.Protocol;
using Deckster.Core.Games.Common;

namespace Deckster.Client.Games.ChatRoom;

/**
 * Autogenerated by really, really eager small hamsters.
*/

[DebuggerDisplay("ChatRoomClient {PlayerData}")]
public class ChatRoomGeneratedClient(IClientChannel channel) : GameClient(channel)
{
    public event Action<ChatNotification>? PlayerSaid;

    protected override void OnNotification(DecksterNotification notification)
    {
        try
        {
            switch (notification)
            {
                case ChatNotification m:
                    PlayerSaid?.Invoke(m);
                    return;
                default:
                    return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

public static class ChatRoomGeneratedClientExtensions
{
}

public static class ChatRoomGeneratedClientDecksterClientExtensions
{
    public static GameApi<ChatRoomGeneratedClient> ChatRoom(this DecksterClient client)
    {
        return new GameApi<ChatRoomGeneratedClient>(client.BaseUri.Append("chatroom"), client.Token, c => new ChatRoomGeneratedClient(c));
    }
}
