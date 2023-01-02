using System.Collections.Concurrent;
using Deckster.Client.Communication;
using Deckster.Client.Communication.Handshake;
using Deckster.Client.Core;
using Deckster.Client.Core.Domain;
using Deckster.Client.Core.Games;
using Deckster.Client.CrazyEights;
using Deckster.CrazyEights;

namespace Deckster.Server.Games.CrazyEights;

public class CrazyEightsGameHost
{
    private readonly ILogger _logger;
    public bool IsStarted => _game != null;
    public Guid Id { get; } = Guid.NewGuid();

    private CrazyEightsGame? _game;
    private readonly CrazyEightsRepo _repo;
    private readonly ConcurrentDictionary<Guid, IDecksterChannel> _communicators = new();
    
    public CrazyEightsGameHost(CrazyEightsRepo repo)
    {
        _logger = Log.Factory.CreateLogger($"{nameof(CrazyEightsGameHost)} {Id}");
        _repo = repo;
    }

    public void Add(IDecksterChannel channel)
    {
        if (IsStarted)
        {
            return;
        }

        channel.OnMessage += OnMessage;
        channel.OnDisconnected += OnDisconnected;
        _communicators[channel.PlayerData.PlayerId] = channel;
    }

    private Task OnDisconnected(IDecksterChannel channel)
    {
        _logger.LogInformation("{player} disconnected", channel.PlayerData.Name);
        _communicators.Remove(channel.PlayerData.PlayerId, out _);
        return Task.CompletedTask;
    }

    private async void OnMessage(IDecksterChannel c, byte[] bytes)
    {
        var message = DecksterJson.Deserialize<CrazyEightsCommand>(bytes);
        await (message switch
        {
            PutCardCommand m => PutCardAsync(c, m),
            PutEightCommand m => PutEightAsync(c, m),
            DrawCardCommand m => DrawCardAsync(c, m),
            PassCommand m => PassAsync(c, m),
            StartCommand m => StartAsync(c, m),
            _ => Task.CompletedTask
        });
    }

    private async Task PassAsync(IDecksterChannel channel, PassCommand command)
    {
        var result = _game.Pass(channel.PlayerData.PlayerId);
        await HandleResultAsync(channel, command, result);
    }

    private async Task DrawCardAsync(IDecksterChannel channel, DrawCardCommand command)
    {
        _logger.LogInformation("{player} draws card", channel.PlayerData.Name);
        var result = _game.DrawCardFromStockPile(channel.PlayerData.PlayerId);
        await HandleResultAsync(channel, command, result);
    }

    private async Task PutEightAsync(IDecksterChannel channel, PutEightCommand command)
    {
        var result = _game.PutEight(channel.PlayerData.PlayerId, command.Card, command.NewSuit);
        await HandleResultAsync(channel, command, result);
    }

    private async Task PutCardAsync(IDecksterChannel channel, PutCardCommand command)
    {
        _logger.LogInformation("{player} put card {card}", channel.PlayerData.Name, command.Card);
        var result = _game.PutCardOnDiscardPile(channel.PlayerData.PlayerId, command.Card);
        
        await HandleResultAsync(channel, command, result);
    }
    
    private async Task HandleResultAsync(IDecksterChannel channel, CrazyEightsCommand command, CommandResult result)
    {
        await channel.RespondAsync(result);
        if (result is SuccessResult)
        {
            var playerData = channel.PlayerData;
            await BroadcastFromAsync(playerData.PlayerId, CreateBroadcastMessage(playerData.PlayerId, command));
            switch (_game.State)
            {
                case GameState.Finished:
                    await Task.WhenAll(_communicators.Select(c => c.Value.SendAsync(
                        new GameEndedMessage
                        {
                            Players = _game.Players.Select(p => new PlayerData
                            {
                                PlayerId = p.Id,
                                Name = p.Name
                            }).ToList()
                        }))
                    );
                    break;
                default:
                {
                    var currentPlayerId = _game.CurrentPlayer.Id;
                    if (currentPlayerId == channel.PlayerData.PlayerId)
                    {
                        return;
                    }
                    var state = _game.GetStateFor(_game.CurrentPlayer.Id);
                    await _communicators[_game.CurrentPlayer.Id].SendAsync(new ItsYourTurnMessage { PlayerViewOfGame = state });
                    break;
                }
            }
        }
    }

    private Task BroadcastFromAsync<TMessage>(Guid playerId, TMessage message) where TMessage : CrazyEightsMessage
    {
        var communiactors = _communicators.Values.Where(c => c.PlayerData.PlayerId != playerId);
        return Task.WhenAll(communiactors.Select(c => c.SendAsync<CrazyEightsMessage>(message)));
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

    private async Task<CommandResult> StartAsync(IDecksterChannel channel, StartCommand command)
    {
        if (IsStarted)
        {
            return new FailureResult("Game already started");
        }
        await StartAsync();
        return new SuccessResult();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var players = _communicators.Select(c => new CrazyEightsPlayer
        {
            Id = c.Value.PlayerData.PlayerId,
            Name = c.Value.PlayerData.Name,
        }).ToArray();
        _game = new CrazyEightsGame(Deck.Default.Shuffle(), players);

        await Task.WhenAll(_communicators.Select(c => c.Value.SendAsync(
            new GameStartedMessage
            {
                PlayerViewOfGame = _game.GetStateFor(c.Value.PlayerData.PlayerId)
            }, cancellationToken))
        );
        
        var currentPlayerId = _game.CurrentPlayer.Id;
        
        await _communicators[currentPlayerId].SendAsync(new ItsYourTurnMessage
        {
            PlayerViewOfGame = _game.GetStateFor(currentPlayerId)
        }, cancellationToken);
    }
}