using Deckster.Core.Games.ChatRoom;
using System;
using System.Diagnostics;
using Deckster.Client.Communication;
using Deckster.Core.Protocol;
using Deckster.Core.Games.Common;

namespace Deckster.Client.Games.ChatRoom;

/**
 * Autogenerated by really, really eager small hamsters.
*/

[DebuggerDisplay("ChatRoomClient {PlayerData}")]
public class ChatRoomClient(IClientChannel channel) : GameClient(channel)
{
    public event Action<ChatNotification>? PlayerSaid;

    public Task<ChatResponse> ChatAsync(SendChatRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<ChatResponse>(request, false, cancellationToken);
    }

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

public static class ChatRoomClientConveniences
{
    public static async Task ChatAsync(this ChatRoomClient self, String message, CancellationToken cancellationToken = default)
    {
        var request = new SendChatRequest{ Message = message };
        var response = await self.SendAsync<ChatResponse>(request, true, cancellationToken);
    }
}

public static class ChatRoomClientDecksterClientExtensions
{
    public static GameApi<ChatRoomClient> ChatRoom(this DecksterClient client)
    {
        return new GameApi<ChatRoomClient>(client.BaseUri.Append("chatroom"), client.Token, c => new ChatRoomClient(c));
    }
}
