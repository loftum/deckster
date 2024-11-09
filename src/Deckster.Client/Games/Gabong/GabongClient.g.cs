using Deckster.Core.Games.Gabong;
using Deckster.Core.Games.Common;
using System;
using System.Diagnostics;
using Deckster.Core.Communication;
using Deckster.Core.Protocol;
using Deckster.Core.Extensions;

namespace Deckster.Client.Games.Gabong;

/**
 * Autogenerated by really, really eager small hamsters.
*/

[DebuggerDisplay("GabongClient {PlayerData}")]
public class GabongClient(IClientChannel channel) : GameClient(channel)
{
    public event Action<GameStartedNotification>? GameStarted;
    public event Action<PlayerPutCardNotification>? PlayerPutCard;
    public event Action<PlayerDrewCardNotification>? PlayerDrewCard;
    public event Action<PlayerDrewPenaltyCardNotification>? PlayerDrewPenaltyCard;
    public event Action<GameEndedNotification>? GameEnded;
    public event Action<RoundStartedNotification>? RoundStarted;
    public event Action<RoundEndedNotification>? RoundEnded;
    public event Action<PlayerLostTheirTurnNotification>? PlayerLostTheirTurn;

    public Task<PlayerViewOfGame> PutCardAsync(PutCardRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<PlayerViewOfGame>(request, false, cancellationToken);
    }

    public Task<PlayerViewOfGame> DrawCardAsync(DrawCardRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<PlayerViewOfGame>(request, false, cancellationToken);
    }

    public Task<PlayerViewOfGame> PassAsync(PassRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<PlayerViewOfGame>(request, false, cancellationToken);
    }

    public Task<PlayerViewOfGame> PlayGabongAsync(PlayGabongRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<PlayerViewOfGame>(request, false, cancellationToken);
    }

    public Task<PlayerViewOfGame> PlayBongaAsync(PlayBongaRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<PlayerViewOfGame>(request, false, cancellationToken);
    }

    protected override void OnNotification(DecksterNotification notification)
    {
        try
        {
            switch (notification)
            {
                case GameStartedNotification m:
                    GameStarted?.Invoke(m);
                    return;
                case PlayerPutCardNotification m:
                    PlayerPutCard?.Invoke(m);
                    return;
                case PlayerDrewCardNotification m:
                    PlayerDrewCard?.Invoke(m);
                    return;
                case PlayerDrewPenaltyCardNotification m:
                    PlayerDrewPenaltyCard?.Invoke(m);
                    return;
                case GameEndedNotification m:
                    GameEnded?.Invoke(m);
                    return;
                case RoundStartedNotification m:
                    RoundStarted?.Invoke(m);
                    return;
                case RoundEndedNotification m:
                    RoundEnded?.Invoke(m);
                    return;
                case PlayerLostTheirTurnNotification m:
                    PlayerLostTheirTurn?.Invoke(m);
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

public static class GabongClientConveniences
{
    public static async Task<(List<Card> cards, Card topOfPile, Suit currentSuit, int stockPileCount, int discardPileCount, Guid lastPlayMadeByPlayerId, GabongPlay lastPlay, List<OtherGabongPlayer> otherPlayers, List<Guid> playersOrder, List<Card> cardsAdded)> PutCardAsync(this GabongClient self, Card card, Nullable<Suit> newSuit, CancellationToken cancellationToken = default)
    {
        var request = new PutCardRequest{ Card = card, NewSuit = newSuit };
        var response = await self.SendAsync<PlayerViewOfGame>(request, true, cancellationToken);
        return (response.Cards, response.TopOfPile, response.CurrentSuit, response.StockPileCount, response.DiscardPileCount, response.LastPlayMadeByPlayerId, response.LastPlay, response.OtherPlayers, response.PlayersOrder, response.CardsAdded);
    }
    public static async Task<(List<Card> cards, Card topOfPile, Suit currentSuit, int stockPileCount, int discardPileCount, Guid lastPlayMadeByPlayerId, GabongPlay lastPlay, List<OtherGabongPlayer> otherPlayers, List<Guid> playersOrder, List<Card> cardsAdded)> DrawCardAsync(this GabongClient self, CancellationToken cancellationToken = default)
    {
        var request = new DrawCardRequest{  };
        var response = await self.SendAsync<PlayerViewOfGame>(request, true, cancellationToken);
        return (response.Cards, response.TopOfPile, response.CurrentSuit, response.StockPileCount, response.DiscardPileCount, response.LastPlayMadeByPlayerId, response.LastPlay, response.OtherPlayers, response.PlayersOrder, response.CardsAdded);
    }
    public static async Task<(List<Card> cards, Card topOfPile, Suit currentSuit, int stockPileCount, int discardPileCount, Guid lastPlayMadeByPlayerId, GabongPlay lastPlay, List<OtherGabongPlayer> otherPlayers, List<Guid> playersOrder, List<Card> cardsAdded)> PassAsync(this GabongClient self, CancellationToken cancellationToken = default)
    {
        var request = new PassRequest{  };
        var response = await self.SendAsync<PlayerViewOfGame>(request, true, cancellationToken);
        return (response.Cards, response.TopOfPile, response.CurrentSuit, response.StockPileCount, response.DiscardPileCount, response.LastPlayMadeByPlayerId, response.LastPlay, response.OtherPlayers, response.PlayersOrder, response.CardsAdded);
    }
    public static async Task<(List<Card> cards, Card topOfPile, Suit currentSuit, int stockPileCount, int discardPileCount, Guid lastPlayMadeByPlayerId, GabongPlay lastPlay, List<OtherGabongPlayer> otherPlayers, List<Guid> playersOrder, List<Card> cardsAdded)> PlayGabongAsync(this GabongClient self, CancellationToken cancellationToken = default)
    {
        var request = new PlayGabongRequest{  };
        var response = await self.SendAsync<PlayerViewOfGame>(request, true, cancellationToken);
        return (response.Cards, response.TopOfPile, response.CurrentSuit, response.StockPileCount, response.DiscardPileCount, response.LastPlayMadeByPlayerId, response.LastPlay, response.OtherPlayers, response.PlayersOrder, response.CardsAdded);
    }
    public static async Task<(List<Card> cards, Card topOfPile, Suit currentSuit, int stockPileCount, int discardPileCount, Guid lastPlayMadeByPlayerId, GabongPlay lastPlay, List<OtherGabongPlayer> otherPlayers, List<Guid> playersOrder, List<Card> cardsAdded)> PlayBongaAsync(this GabongClient self, CancellationToken cancellationToken = default)
    {
        var request = new PlayBongaRequest{  };
        var response = await self.SendAsync<PlayerViewOfGame>(request, true, cancellationToken);
        return (response.Cards, response.TopOfPile, response.CurrentSuit, response.StockPileCount, response.DiscardPileCount, response.LastPlayMadeByPlayerId, response.LastPlay, response.OtherPlayers, response.PlayersOrder, response.CardsAdded);
    }
}

public static class GabongClientDecksterClientExtensions
{
    public static GameApi<GabongClient> Gabong(this DecksterClient client)
    {
        return new GameApi<GabongClient>(client.BaseUri.Append("gabong"), client.Token, c => new GabongClient(c));
    }
}
