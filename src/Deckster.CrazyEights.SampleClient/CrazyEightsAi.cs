using Deckster.Core;
using Deckster.Core.Domain;
using Deckster.CrazyEights.Game;
using Microsoft.Extensions.Logging;

namespace Deckster.CrazyEights.SampleClient;

public class CrazyEightsAi
{
    private readonly ILogger _logger;
    
    private PlayerViewOfGame _view = new();
    private readonly CrazyEightsClient _client;
    private bool _gameEnded;

    public CrazyEightsAi(CrazyEightsClient client)
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

    private void GameEnded(GameEndedMessage message)
    {
        _logger.LogInformation("Game ended");
        _gameEnded = true;
    }

    private void GameStarted(GameStartedMessage message)
    {
        _view = message.PlayerViewOfGame;
    }

    private async void ItsMyTurn(ItsYourTurnMessage message)
    {
        var cards = message.PlayerViewOfGame.Cards;
        _logger.LogInformation("It's my turn. I have: {cards}", string.Join(", ", cards));
        _view = message.PlayerViewOfGame;
        

        if (TryGetCard(out var card))
        {
            _logger.LogInformation("Putting card: {card}", card);
            var r = await _client.PutCardAsync(card);
            _logger.LogInformation("Got response {r}", r.GetType().Name);
            return;
        }

        for (var ii = 0; ii < 3; ii++)
        {
            card = await _client.DrawCardAsync();
            _logger.LogInformation("Drawing card: {card}", card);
            _view.Cards.Add(card);
            if (TryGetCard(out card))
            {
                _logger.LogInformation("Putting card: {card}", card);
                await _client.PutCardAsync(card);
                return;
            }
        }

        _logger.LogInformation("Passing");
        await _client.PassAsync();
    }

    private bool TryGetCard(out Card card)
    {
        card = default;
        foreach (var c in _view.Cards.Where(c => c.Rank == _view.TopOfPile.Rank || c.Suit == _view.CurrentSuit))
        {
            card = c;
            return true;
        }

        return false;
    }

    private void PlayerPutEight(PlayerPutEightMessage message)
    {
        _logger.LogTrace("{playerId} put eight {card}", message.PlayerId, message.Card);
    }

    private void PlayerPutCard(PlayerPutCardMessage message)
    {
        _logger.LogTrace("{playerId} put {card}", message.PlayerId, message.Card);
    }

    private void PlayerDrewCard(PlayerDrewCardMessage message)
    {
        _logger.LogInformation("Player drew card: {playerId}", message.PlayerId);
    }

    private void PlayerPassed(PlayerPassedMessage message)
    {
        _logger.LogInformation("Player passed: {playerId}", message.PlayerId);
    }

    public async Task PlayAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !_gameEnded)
        {
            await Task.Delay(500, cancellationToken);
        }
    }
}