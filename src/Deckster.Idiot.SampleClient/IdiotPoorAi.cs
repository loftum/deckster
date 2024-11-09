using Deckster.Client.Games.Idiot;
using Deckster.Core.Games.Idiot;

namespace Deckster.Idiot.SampleClient;

public class IdiotPoorAi
{
    private readonly TaskCompletionSource _tcs = new();

    public IdiotPoorAi(IdiotClient client)
    {
        
        
        client.PlayerSwappedCards += PlayerSwappedCards;
        client.ItsTimeToSwapCards += SwapCards;
        client.PlayerIsReady += PlayerIsReady;

        client.GameHasStarted += GameHasStarted;
        client.GameEnded += GameEnded;
        client.ItsYourTurn += ItsMyTurn;
        client.PlayerDrewCards += PlayerDrewCards;
        client.PlayerPutCards += PlayerPutCards;
        client.DiscardPileFlushed += DiscardPileFlushed;
        client.PlayerIsDone += PlayerIsDone;
        
        client.PlayerAttemptedPuttingCard += PlayerAttemptedPuttingCards;
        client.PlayerPulledInDiscardPile += PlayerPulledInDiscardPile;
    }

    private void ItsMyTurn(ItsYourTurnNotification obj)
    {
        
    }

    private void PlayerPulledInDiscardPile(PlayerPulledInDiscardPileNotification obj)
    {
        
    }

    private void PlayerAttemptedPuttingCards(PlayerAttemptedPuttingCardNotification n)
    {
        
    }

    private void PlayerSwappedCards(PlayerSwappedCardsNotification obj)
    {
        
    }

    private void PlayerDrewCards(PlayerDrewCardsNotification n)
    {
        
    }

    private void PlayerIsDone(PlayerIsDoneNotification n)
    {
        
    }

    private void PlayerPutCards(PlayerPutCardsNotification obj)
    {
        
        
    }

    private void PlayerIsReady(PlayerIsReadyNotification n)
    {
        
    }

    private void DiscardPileFlushed(DiscardPileFlushedNotification obj)
    {
        
    }

    private void GameEnded(GameEndedNotification obj)
    {
        
    }

    private void GameHasStarted(GameStartedNotification n)
    {
        
    }

    private void SwapCards(ItsTimeToSwapCardsNotification n)
    {
        
    }

    public Task PlayAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.Register(_tcs.SetCanceled);
        return _tcs.Task;
    }
}