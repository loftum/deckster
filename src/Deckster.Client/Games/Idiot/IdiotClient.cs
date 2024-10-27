using Deckster.Client.Communication;
using Deckster.Client.Protocol;

namespace Deckster.Client.Games.Idiot;

public class IdiotClient : GameClient<IdiotRequest, IdiotResponse>
{
    public IdiotClient(IClientChannel channel) : base(channel)
    {
    }

    protected override void OnNotification(DecksterNotification notification)
    {
        
    }
}