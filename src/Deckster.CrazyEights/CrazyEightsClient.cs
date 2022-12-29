using System.Text.Json;
using Deckster.Communication;
using Deckster.Core;
using Deckster.Core.Domain;
using Deckster.Core.Games;
using Deckster.CrazyEights.Game;
using Microsoft.Extensions.Logging;

namespace Deckster.CrazyEights;

public class CrazyEightsClient
{
    private readonly ILogger _logger = Log.Factory.CreateLogger<CrazyEightsClient>();
    public event Func<PlayerPutCardMessage, Task>? PlayerPutCard;
    public event Func<PlayerPutEightMessage, Task>? PlayerPutEight;
    public event Func<PlayerDrewCardMessage, Task>? PlayerDrewCard;
    public event Func<PlayerPassedMessage, Task>? PlayerPassed;
    public event Func<ItsYourTurnMessage, Task>? ItsYourTurn;
    public event Func<GameStartedMessage, Task>? GameStarted;

    private readonly IDecksterCommunicator _communicator;

    public CrazyEightsClient(IDecksterCommunicator communicator)
    {
        _communicator = communicator;
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
        var result = await SendAsync<DrawCardCommand, CardResult>(new DrawCardCommand(), cancellationToken);
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
        _logger.LogInformation("Sending {type}", typeof(TCommand));
        await _communicator.SendAsync(command, DecksterJson.Options, cancellationToken);
        _logger.LogInformation("Waiting for response");
        var result = await _communicator.ReceiveAsync<CommandResult>(DecksterJson.Options, cancellationToken);
        return result switch
        {
            null => throw new Exception("Result is null. Wat"),
            FailureResult r => throw new Exception(r.Message),
            TResult r => r,
            _ => throw new Exception($"Unknown result '{result.GetType().Name}'")
        };
    }

    private Task HandleMessageAsync(IDecksterCommunicator communicator, byte[] bytes)
    {
        try
        {
            var message = JsonSerializer.Deserialize<CrazyEightsMessage>(bytes, DecksterJson.Options);
            return message switch
            {
                PlayerPutCardMessage m when PlayerPutCard != null => PlayerPutCard(m),
                PlayerPutEightMessage m when PlayerPutEight != null => PlayerPutEight(m),
                PlayerDrewCardMessage m when PlayerDrewCard != null => PlayerDrewCard(m),
                PlayerPassedMessage m when PlayerPassed != null => PlayerPassed(m),
                ItsYourTurnMessage m when ItsYourTurn != null => ItsYourTurn(m),
                _ => Task.CompletedTask
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Task.CompletedTask;
        }
    }

    public async Task DisconnectAsync()
    {
        await _communicator.DisconnectAsync();
    }
}