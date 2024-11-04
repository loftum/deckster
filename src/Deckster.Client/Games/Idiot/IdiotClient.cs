using Deckster.Client.Communication;
using Deckster.Core.Protocol;

namespace Deckster.Client.Games.Idiot;

public class IdiotClient : GameClient
{
    public IdiotClient(IClientChannel channel) : base(channel)
    {
    }

    protected override void OnNotification(DecksterNotification notification)
    {
        
    }
}