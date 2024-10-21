using Deckster.Client.Games.Common;
using Deckster.Client.Games.CrazyEights;
using Deckster.Client.Logging;
using Microsoft.Extensions.Logging;

namespace Deckster.CrazyEights.SampleClient;

public class CrazyEightsPoorAi
{
    private readonly ILogger _logger;
    
    private PlayerViewOfGame _view = new();
    private readonly CrazyEightsClient _client;
    private bool _gameEnded;

    public CrazyEightsPoorAi(CrazyEightsClient client)
    {
        _client = client;
        _logger = Log.Factory.CreateLogger(client.PlayerData.Name);
        client.PlayerPassed += PlayerPassed;
        client.PlayerDrewCard += PlayerDrewCard;
        client.PlayerPutCard += PlayerPutCard;
        client.PlayerPutEight += PlayerPutEight;
        client.ItsYourTurn += ItsMyTurn;
        client.GameStarted += GameStarted;
        client.GameEnded += GameEnded;
    }

    private void GameEnded(GameEndedNotification notification)
    {
        _logger.LogInformation($"Game ended. Players: [{string.Join(", ", notification.Players.Select(p => p.Name))}]");
        _gameEnded = true;
    }

    private void GameStarted(GameStartedNotification notification)
    {
        _logger.LogInformation("Game started");
        _view = notification.PlayerViewOfGame;
    }

    private int _turn;

    private async void ItsMyTurn(ItsYourTurnNotification notification)
    {
        var turn = _turn++;
        try
        {
            var cards = notification.PlayerViewOfGame.Cards;
            
            _logger.LogInformation("It's my turn. Top: {top} ({suit}). I have: {cards} ({turn})",
                notification.PlayerViewOfGame.TopOfPile,
                notification.PlayerViewOfGame.CurrentSuit.Display(),
                string.Join(", ", cards),
                turn);
            
            _view = notification.PlayerViewOfGame;

            if (TryGetCard(out var card))
            {
                try
                {
                    _logger.LogInformation("Putting card: {card} ({turn})", card, turn);
                    var r = await _client.PutCardAsync(card);
                    _logger.LogInformation("Result: {result}", r.GetType().Name);
                }
                catch (Exception e)
                {
                    _logger.LogError("{message} ({turn})", e.Message, turn);
                    throw;
                }

                return;
            }

            for (var ii = 0; ii < 3; ii++)
            {
                _logger.LogTrace("ii:{ii}, ({turn})", ii, turn);
                card = await _client.DrawCardAsync();
                _logger.LogInformation("Drawing card: {card} ({turn})", card, turn);
                _view.Cards.Add(card);
                if (TryGetCard(out card))
                {
                    try
                    {
                        _logger.LogInformation("Putting card: {card} ({turn})", card, turn);
                        var r = await _client.PutCardAsync(card);
                        _logger.LogInformation("Result: {result}", r.GetType().Name);
                        return;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("{message} ({turn})", e.Message, turn);
                        throw;
                    }

                    return;
                }
            }

            _logger.LogInformation("Passing ({turn})", turn);
            await _client.PassAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Argh ({turn})", turn);
            throw;
        }
        _logger.LogInformation("Done ({turn})", turn);
    }

    private bool TryGetCard(out Card card)
    {
        _logger.LogTrace("Try get card");
        card = default;
        foreach (var c in _view.Cards.Where(c => c.Rank == _view.TopOfPile.Rank || c.Suit == _view.CurrentSuit))
        {
            card = c;
            return true;
        }
        _logger.LogTrace("Try get card: no");
        return false;
    }

    private void PlayerPutEight(PlayerPutEightNotification notification)
    {
        _logger.LogTrace("{playerId} put eight {card}", notification.PlayerId, notification.Card);
    }

    private void PlayerPutCard(PlayerPutCardNotification notification)
    {
        _logger.LogTrace("{playerId} put {card}", notification.PlayerId, notification.Card);
    }

    private void PlayerDrewCard(PlayerDrewCardNotification notification)
    {
        _logger.LogTrace("Player drew card: {playerId}", notification.PlayerId);
    }

    private void PlayerPassed(PlayerPassedNotification notification)
    {
        _logger.LogTrace("Player passed: {playerId}", notification.PlayerId);
    }

    public async Task PlayAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !_gameEnded)
        {
            await Task.Delay(500, cancellationToken);
        }
    }
}