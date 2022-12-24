using System.Collections.Concurrent;
using System.Text.Json;
using Deckster.Communication;
using Deckster.Communication.Handshake;
using Deckster.Core;
using Deckster.Core.Domain;
using Deckster.Core.Games;
using Deckster.CrazyEights;
using Deckster.CrazyEights.Game;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsGameHost
{
    private readonly ILogger _logger;
    public bool IsStarted => _game != null;
    public Guid Id { get; } = Guid.NewGuid();

    private CrazyEightsGame? _game;
    private readonly CrazyEightsRepo _repo;
    private readonly ConcurrentDictionary<Guid, IDecksterCommunicator> _communicators = new();
    
    public CrazyEightsGameHost(CrazyEightsRepo repo)
    {
        _logger = Log.Factory.CreateLogger($"{nameof(CrazyEightsGameHost)} {Id}");
        _repo = repo;
    }

    public void Add(IDecksterCommunicator communicator)
    {
        if (IsStarted)
        {
            return;
        }

        communicator.OnMessage += OnMessage;
        communicator.OnDisconnected += OnDisconnected;
        _communicators[communicator.PlayerData.PlayerId] = communicator;
    }

    private Task OnDisconnected(IDecksterCommunicator communicator)
    {
        _logger.LogInformation("{player} disconnected", communicator.PlayerData.Name);
        _communicators.Remove(communicator.PlayerData.PlayerId, out _);
        return Task.CompletedTask;
    }

    private Task OnMessage(IDecksterCommunicator c, Stream stream, byte[] bytes)
    {
        var message = JsonSerializer.Deserialize<CrazyEightsCommand>(bytes, DecksterJson.Options);
        return message switch
        {
            PutCardCommand m => PutCardAsync(c, m, stream),
            PutEightCommand m => PutEightAsync(c, m, stream),
            DrawCardCommand m => DrawCardAsync(c, m, stream),
            PassCommand m => PassAsync(c, m, stream),
            _ => Task.CompletedTask
        };
    }

    private async Task PassAsync(IDecksterCommunicator communicator, PassCommand command, Stream stream)
    {
        var result = _game.Pass(communicator.PlayerData.PlayerId);
        await HandleResultAsync(communicator.PlayerData, command, stream, result);
    }

    private async Task DrawCardAsync(IDecksterCommunicator communicator, DrawCardCommand command, Stream stream)
    {
        var result = _game.DrawCardFromStockPile(communicator.PlayerData.PlayerId);
        await HandleResultAsync(communicator.PlayerData, command, stream, result);
    }

    private async Task PutEightAsync(IDecksterCommunicator communicator, PutEightCommand command, Stream stream)
    {
        var result = _game.PutEight(communicator.PlayerData.PlayerId, command.Card, command.NewSuit);
        await stream.SendJsonAsync(result, DecksterJson.Options);
        await HandleResultAsync(communicator.PlayerData, command, stream, result);
    }

    private async Task PutCardAsync(IDecksterCommunicator communicator, PutCardCommand command, Stream stream)
    {
        var result = _game.PutCardOnDiscardPile(communicator.PlayerData.PlayerId, command.Card);
        await HandleResultAsync(communicator.PlayerData, command, stream, result);
    }
    
    private async Task HandleResultAsync(PlayerData playerData, CrazyEightsCommand command, Stream stream, CommandResult result)
    {
        await stream.SendJsonAsync(result, DecksterJson.Options);
        if (result is SuccessResult)
        {
            await BroadCastFrom(playerData.PlayerId, CreateBroadcastMessage(playerData.PlayerId, command));
            var state = _game.GetStateFor(_game.CurrentPlayer.Id);
            await _communicators[_game.CurrentPlayer.Id].SendJsonAsync<CrazyEightsMessage>(new ItsYourTurnMessage { PlayerViewOfGame = state }, DecksterJson.Options);
        }
    }

    private Task BroadCastFrom<TMessage>(Guid playerId, TMessage message) where TMessage : CrazyEightsMessage
    {
        var communiactors = _communicators.Values.Where(c => c.PlayerData.PlayerId != playerId);
        return Task.WhenAll(communiactors.Select(c => c.SendJsonAsync<CrazyEightsMessage>(message, DecksterJson.Options)));
    }
    
    private static CrazyEightsMessage CreateBroadcastMessage(Guid playerId, CrazyEightsCommand command)
    {
        return command switch
        {
            PutCardCommand p => new PlayerPutCardMessage {PlayerId = playerId, Card = p.Card},
            PutEightCommand p => new PlayerPutEightMessage {PlayerId = playerId, Card = p.Card, NewSuit = p.NewSuit},
            DrawCardCommand p => new PlayerDrewCardMessage {PlayerId = playerId},
            PassCommand p => new PlayerPassedMessage {PlayerId = playerId},
            _ => throw new Exception($"Unknown broadcast message for '{command.GetType().Name}'")
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var players = _communicators.Select(c => new CrazyEightsPlayer
        {
            Id = c.Value.PlayerData.PlayerId,
            Name = c.Value.PlayerData.Name,
        }).ToArray();
        _game = new CrazyEightsGame(Deck.Default, players);
        
        await Task.WhenAll(_communicators.Select(c => c.Value.SendJsonAsync<CrazyEightsMessage>(
            new GameStartedMessage
            {
                PlayerViewOfGame = _game.GetStateFor(c.Value.PlayerData.PlayerId)
            }, DecksterJson.Options, cancellationToken))
        );
        
        var currentPlayerId = _game.CurrentPlayer.Id;
        
        await _communicators[currentPlayerId].SendJsonAsync<CrazyEightsMessage>(new ItsYourTurnMessage
        {
            PlayerViewOfGame = _game.GetStateFor(currentPlayerId)
        }, DecksterJson.Options, cancellationToken);
    }
}