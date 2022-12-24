using Deckster.Core.Domain;
using Deckster.CrazyEights;
using Deckster.CrazyEights.Game;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsAi
{
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
        _view = message.PlayerViewOfGame;

        if (TryGetCard(out var card))
        {
            await _client.PutCardAsync(card);
            return;
        }

        for (var ii = 0; ii < 3; ii++)
        {
            card = await _client.DrawCardAsync();
            _view.Cards.Add(card);
            if (TryGetCard(out card))
            {
                await _client.PutCardAsync(card);
                return;
            }
        }
        
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
        return Task.CompletedTask;
    }

    private Task PlayerPutCard(PlayerPutCardMessage messag)
    {
        return Task.CompletedTask;
    }

    private Task PlayerDrewCard(PlayerDrewCardMessage message)
    {
        return Task.CompletedTask;
    }

    private Task PlayerPassed(PlayerPassedMessage message)
    {
        return Task.CompletedTask;
    }
    
}