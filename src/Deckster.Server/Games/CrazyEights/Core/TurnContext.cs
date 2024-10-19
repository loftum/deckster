using Deckster.Client.Protocol;

namespace Deckster.Server.Games.CrazyEights.Core;

public class TurnContext
{
    public DecksterRequest Request { get; }
    public DecksterResponse? Response { get; set; }
    public List<DecksterNotification> Notifications { get; } = [];

    public TurnContext(DecksterRequest request)
    {
        Request = request;
    }

    public void SetResponse(DecksterResponse response)
    {
        Response = response;
    }

    public void AddNotification(DecksterNotification notification)
    {
        Notifications.Add(notification);
    }
}