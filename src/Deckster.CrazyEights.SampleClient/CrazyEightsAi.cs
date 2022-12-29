using Deckster.Core;
using Deckster.Core.Domain;
using Deckster.CrazyEights.Game;
using Microsoft.Extensions.Logging;

namespace Deckster.CrazyEights.SampleClient;

public class CrazyEightsAi
{
    private readonly ILogger _logger = Log.Factory.CreateLogger(nameof(CrazyEightsAi));
    
    private PlayerViewOfGame _view = new();
    private readonly CrazyEightsClient _client;

    public CrazyEightsAi(CrazyEightsClient client)
    {
        _client = client;
        client.PlayerPassed += PlayerPassed;
        client.PlayerDrewCard += PlayerDrewCard;
        client.PlayerPutCard += PlayerPutCard;
        client.PlayerPutEight += PlayerPutEight;
        client.ItsYourTurn += ItsMyTurn;
        client.GameStarted += GameStarted;
    }

    private Task GameStarted(GameStartedMessage message)
    {
        _view = message.PlayerViewOfGame;
        return Task.CompletedTask;
    }

    private async Task ItsMyTurn(ItsYourTurnMessage message)
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

    private Task PlayerPutEight(PlayerPutEightMessage message)
    {
        _logger.LogInformation("Player put eight: {playerId}: {card}", message.PlayerId, message.Card);
        return Task.CompletedTask;
    }

    private Task PlayerPutCard(PlayerPutCardMessage message)
    {
        _logger.LogInformation("Player put card: {playerId}: {card}", message.PlayerId, message.Card);
        return Task.CompletedTask;
    }

    private Task PlayerDrewCard(PlayerDrewCardMessage message)
    {
        _logger.LogInformation("Player drew card: {playerId}", message.PlayerId);
        return Task.CompletedTask;
    }

    private Task PlayerPassed(PlayerPassedMessage message)
    {
        _logger.LogInformation("Player passed: {playerId}", message.PlayerId);
        return Task.CompletedTask;
    }

    public async Task PlayAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(500, cancellationToken);
        }
    }
}