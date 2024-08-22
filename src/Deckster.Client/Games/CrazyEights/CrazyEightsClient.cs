using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Client.Games.Common;
using Deckster.Client.Logging;
using Deckster.Client.Protocol;
using Microsoft.Extensions.Logging;

namespace Deckster.Client.Games.CrazyEights;

public class CrazyEightsClient
{
    private readonly IClientChannel _channel;
    private readonly SemaphoreSlim _semaphore = new(1,1);
    private readonly ILogger _logger;
    
    public event Action<PlayerPutCardMessage>? PlayerPutCard;
    public event Action<PlayerPutEightMessage>? PlayerPutEight;
    public event Action<PlayerDrewCardMessage>? PlayerDrewCard;
    public event Action<PlayerPassedMessage>? PlayerPassed;
    public event Action<ItsYourTurnMessage>? ItsYourTurn;
    public event Action<GameStartedMessage>? GameStarted;
    public event Action<GameEndedMessage>? GameEnded;

    public PlayerData PlayerData => _channel.PlayerData;

    public CrazyEightsClient(IClientChannel channel)
    {
        _channel = channel;
        _logger = Log.Factory.CreateLogger(channel.PlayerData.Name);
        channel.OnMessage += HandleMessageAsync;
    }

    public Task<DecksterCommandResult> PutCardAsync(Card card, CancellationToken cancellationToken = default)
    {
        var command = new PutCardCommand
        {
            Card = card
        };
        return SendAsync<PutCardCommand, DecksterCommandResult>(command, cancellationToken);
    }

    public Task PutEightAsync(Card card, Suit newSuit, CancellationToken cancellationToken = default)
    {
        var command = new PutEightCommand
        {
            Card = card,
            NewSuit = newSuit
        };
        return SendAsync<PutEightCommand, DecksterCommandResult>(command, cancellationToken);
    }

    public async Task<Card> DrawCardAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Draw card");
        var result = await SendAsync<DrawCardCommand, CardResult>(new DrawCardCommand(), cancellationToken);
        _logger.LogTrace("Draw card: {result}", result.Card);
        return result.Card;
    }

    public Task PassAsync(CancellationToken cancellationToken = default)
    {
        return SendAsync<PassCommand, DecksterCommandResult>(new PassCommand(), cancellationToken);
    }

    private async Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : DecksterCommand
        where TResult : DecksterCommandResult
    {
        _logger.LogTrace("Sending {type}", typeof(TCommand));
        var result = await DoSendAsync(command, cancellationToken);
        _logger.LogTrace("Got response");
        return result switch
        {
            null => throw new Exception("Result is null. Wat"),
            FailureResult r => throw new Exception(r.Message),
            TResult r => r,
            _ => throw new Exception($"Unknown result '{result.GetType().Name}'")
        };
    }

    private async Task<DecksterCommandResult?> DoSendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : DecksterCommand
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            await _channel.SendAsync(command, cancellationToken);
            var result = await _channel.ReceiveAsync<DecksterCommandResult>(cancellationToken);
            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async void HandleMessageAsync(IClientChannel channel, byte[] bytes)
    {
        try
        {
            var message = DecksterJson.Deserialize<CrazyEightsMessage>(bytes);
            switch (message)
            {
                case GameStartedMessage m:
                    GameStarted?.Invoke(m);
                    break;
                case GameEndedMessage m:
                    await _channel.DisconnectAsync();
                    GameEnded?.Invoke(m);
                    break;
                case PlayerPutCardMessage m:
                    PlayerPutCard?.Invoke(m);
                    break;
                case PlayerPutEightMessage m: 
                    PlayerPutEight?.Invoke(m);
                    break;
                case PlayerDrewCardMessage m: 
                    PlayerDrewCard?.Invoke(m);
                    break;
                case PlayerPassedMessage m:
                    PlayerPassed?.Invoke(m);
                    break;
                case ItsYourTurnMessage m:
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

    public async Task DisconnectAsync()
    {
        await _channel.DisconnectAsync();
    }
}