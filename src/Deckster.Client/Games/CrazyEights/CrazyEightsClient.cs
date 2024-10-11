using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Client.Games.Common;
using Deckster.Client.Logging;
using Microsoft.Extensions.Logging;

namespace Deckster.Client.Games.CrazyEights;

public class CrazyEightsClient : GameClient<CrazyEightsRequest, CrazyEightsResponse, CrazyEightsNotification>
{
    private readonly ILogger _logger;
    
    public event Action<PlayerPutCardNotification>? PlayerPutCard;
    public event Action<PlayerPutEightNotification>? PlayerPutEight;
    public event Action<PlayerDrewCardNotification>? PlayerDrewCard;
    public event Action<PlayerPassedNotification>? PlayerPassed;
    public event Action<ItsYourTurnNotification>? ItsYourTurn;
    public event Action<GameStartedNotification>? GameStarted;
    public event Action<GameEndedNotification>? GameEnded;

    public PlayerData PlayerData => Channel.PlayerData;

    public CrazyEightsClient(IClientChannel<CrazyEightsNotification> channel) : base(channel)
    {
        _logger = Log.Factory.CreateLogger(channel.PlayerData.Name);
        channel.OnMessage += HandleMessageAsync;
    }

    public Task<CrazyEightsResponse> PutCardAsync(Card card, CancellationToken cancellationToken = default)
    {
        var request = new PutCardRequest
        {
            Card = card
        };
        return SendAsync(request, cancellationToken);
    }

    public Task<CrazyEightsResponse> PutEightAsync(Card card, Suit newSuit, CancellationToken cancellationToken = default)
    {
        var request = new PutEightRequest
        {
            Card = card,
            NewSuit = newSuit
        };
        return SendAsync(request, cancellationToken);
    }

    public async Task<Card> DrawCardAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Draw card");
        var result = await GetAsync<CardResponse>(new DrawCardRequest(), cancellationToken);
        _logger.LogTrace("Draw card: {result}", result.Card);
        return result.Card;
    }

    public Task<CrazyEightsResponse> PassAsync(CancellationToken cancellationToken = default)
    {
        return SendAsync(new PassRequest(), cancellationToken);
    }

    private async void HandleMessageAsync(CrazyEightsNotification notification)
    {
        try
        {
            switch (notification)
            {
                case GameStartedNotification m:
                    GameStarted?.Invoke(m);
                    break;
                case GameEndedNotification m:
                    await Channel.DisconnectAsync();
                    GameEnded?.Invoke(m);
                    break;
                case PlayerPutCardNotification m:
                    PlayerPutCard?.Invoke(m);
                    break;
                case PlayerPutEightNotification m: 
                    PlayerPutEight?.Invoke(m);
                    break;
                case PlayerDrewCardNotification m: 
                    PlayerDrewCard?.Invoke(m);
                    break;
                case PlayerPassedNotification m:
                    PlayerPassed?.Invoke(m);
                    break;
                case ItsYourTurnNotification m:
                    ItsYourTurn?.Invoke(m);
                    break;
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