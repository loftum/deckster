using System.Text.Json;
using Deckster.Client.Communication;
using Deckster.Client.Communication.Handshake;
using Deckster.Client.Core;
using Deckster.Client.Core.Domain;
using Deckster.Client.Core.Games;
using Deckster.Client.CrazyEights.Game;
using Microsoft.Extensions.Logging;

namespace Deckster.Client.CrazyEights;

public class CrazyEightsClient
{
    private readonly ILogger _logger;
    public event Action<PlayerPutCardMessage>? PlayerPutCard;
    public event Action<PlayerPutEightMessage>? PlayerPutEight;
    public event Action<PlayerDrewCardMessage>? PlayerDrewCard;
    public event Action<PlayerPassedMessage>? PlayerPassed;
    public event Action<ItsYourTurnMessage>? ItsYourTurn;
    public event Action<GameStartedMessage>? GameStarted;
    public event Action<GameEndedMessage>? GameEnded;

    private readonly IDecksterCommunicator _communicator;
    public PlayerData PlayerData => _communicator.PlayerData;

    public CrazyEightsClient(IDecksterCommunicator communicator)
    {
        _communicator = communicator;
        _logger = Log.Factory.CreateLogger(communicator.PlayerData.Name);
        communicator.OnMessage += HandleMessageAsync;
    }

    public Task<CommandResult> PutCardAsync(Card card, CancellationToken cancellationToken = default)
    {
        var command = new PutCardCommand
        {
            Card = card
        };
        return SendAsync<PutCardCommand, CommandResult>(command, cancellationToken);
    }

    public Task PutEightAsync(Card card, Suit newSuit, CancellationToken cancellationToken = default)
    {
        var command = new PutEightCommand
        {
            Card = card,
            NewSuit = newSuit
        };
        return SendAsync<PutEightCommand, CommandResult>(command, cancellationToken);
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
        return SendAsync<PassCommand, CommandResult>(new PassCommand(), cancellationToken);
    }

    private async Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : CrazyEightsCommand
        where TResult : CommandResult
    {
        _logger.LogTrace("Sending {type}", typeof(TCommand));
        await _communicator.SendAsync(command, cancellationToken);
        _logger.LogTrace("Waiting for response");
        var result = await _communicator.ReceiveAsync<CommandResult>(cancellationToken);
        return result switch
        {
            null => throw new Exception("Result is null. Wat"),
            FailureResult r => throw new Exception(r.Message),
            TResult r => r,
            _ => throw new Exception($"Unknown result '{result.GetType().Name}'")
        };
    }

    private async void HandleMessageAsync(IDecksterCommunicator communicator, byte[] bytes)
    {
        try
        {
            var message = JsonSerializer.Deserialize<CrazyEightsMessage>(bytes, DecksterJson.Options);
            switch (message)
            {
                case GameStartedMessage m:
                    GameStarted?.Invoke(m);
                    break;
                case GameEndedMessage m:
                    await _communicator.DisconnectAsync();
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
        await _communicator.DisconnectAsync();
    }
}