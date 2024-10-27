using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Games.Common;
using Deckster.Client.Games.Idiot;
using Deckster.Client.Protocol;
using Deckster.Server.Collections;
using Deckster.Server.Games.Common;

namespace Deckster.Server.Games.Idiot.Core;

public class IdiotGame : GameObject
{
    public int Seed { get; set; }
    public override GameState State => Players.Count(p => p.IsStillPlaying()) > 1 ? GameState.Running : GameState.Finished;
    public int CurrentPlayerIndex { get; set; }
    public IdiotPlayer CurrentPlayer => State == GameState.Finished ? IdiotPlayer.Null : Players[CurrentPlayerIndex];
    /// <summary>
    /// All the (shuffled) cards in the game
    /// </summary>
    public List<Card> Deck { get; init; } = [];
    
    /// <summary>
    /// Where players draw cards from
    /// </summary>
    public List<Card> StockPile { get; init; } = [];
    
    /// <summary>
    /// Where players put cards
    /// </summary>
    public List<Card> DiscardPile { get; init; } = [];
    
    /// <summary>
    /// Pile of garbage, when a user plays a 10 or 4 of same number
    /// </summary>
    public List<Card> GarbagePile { get; init; } = [];

    public Card? TopOfPile => DiscardPile.PeekOrDefault();
    
    /// <summary>
    /// Done players
    /// </summary>
    public List<IdiotPlayer> DonePlayers { get; init; } = [];
    
    /// <summary>
    /// All the players
    /// </summary>
    public List<IdiotPlayer> Players { get; init; } = [];

    public static IdiotGame Create(IdiotGameCreatedEvent created)
    {
        return new IdiotGame
        {
            Id = created.Id,
            StartedTime = created.StartedTime,
            Seed = created.InitialSeed,
            Deck = created.Deck,
            Players = created.Players.Select(p => new IdiotPlayer
            {
                Id = p.Id,
                Name = p.Name
            }).ToList()
        };
    }

    public async Task<DecksterResponse> PutCardsFromHand(Guid playerId, Card[] cards)
    {
        IncrementSeed();
        DecksterResponse response;
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            response = new FailureResponse("It is not your turn");
            await Communication.RespondAsync(playerId, response);
            return response;
        }

        if (cards.Length < 1)
        {
            response = new FailureResponse("You must put at least 1 card");
            await Communication.RespondAsync(playerId, response);
            return response;
        }
        
        if (!player.CardsOnHand.RemoveAll(cards))
        {
            response = new FailureResponse("You don't have all of those cards");
            await Communication.RespondAsync(playerId, response);
            return response;
        }
        
        if (!CardsHaveSameRank(cards, out var rank))
        {
            response = new FailureResponse("All cards must have same rank");
            await Communication.RespondAsync(playerId, response);
            return response;
        }

        var currentRank = TopOfPile?.Rank;
        if (rank < currentRank && rank != 2 && rank != 10)
        {
            response = new FailureResponse($"Rank ({rank}) must be equal to or higher than current rank ({currentRank})");
            await Communication.RespondAsync(playerId, response);
            return response;
        }
        
        DiscardPile.PushRange(cards);

        var discardpileFlushed = false;
        if (rank == 10 || cards.Length == 4)
        {
            GarbagePile.PushRange(DiscardPile);
            DiscardPile.Clear();
            discardpileFlushed = true;
        }
        

        response = new SuccessResponse();
        await Communication.RespondAsync(playerId, response);

        await Communication.NotifyAllAsync(new PlayerPutCardsNotification
        {
            PlayerId = playerId,
            Cards = cards
        });
        
        if (discardpileFlushed)
        {
            await Communication.NotifyAllAsync(new DiscardPileFlushedNotification
            {
                PlayerId = playerId
            }); 
        }

        await MoveToNextPlayerOrFinishAsync();
        
        return response;
    }
    
    private async Task MoveToNextPlayerOrFinishAsync()
    {
        if (State == GameState.Finished)
        {
            await Communication.NotifyAllAsync(new GameEndedNotification());
            return;
        }
        
        MoveToNextPlayer();
        await Communication.NotifyAsync(CurrentPlayer.Id, new ItsYourTurnNotification
        {
            PlayerViewOfGame = GetPlayerViewOfGame(CurrentPlayer)
        });
    }
    
    private PlayerViewOfGame GetPlayerViewOfGame(IdiotPlayer player)
    {
        return new PlayerViewOfGame
        {
            CardsOnHand = player.CardsOnHand,
            TopOfPile = TopOfPile,
            DiscardPileCount = DiscardPile.Count,
            StockPileCount = StockPile.Count,
            OtherPlayers = Players.Where(p => p.Id != player.Id).Select(ToOtherPlayer).ToList()
        };
    }
    
    private static OtherIdiotPlayer ToOtherPlayer(IdiotPlayer player)
    {
        return new OtherIdiotPlayer
        {
            PlayerId = player.Id,
            Name = player.Name,
            CardsOnHandCount = player.CardsOnHand.Count,
            VisibleTableCards = player.VisibleTableCards,
            HiddenTableCardsCount = player.HiddenTableCards.Count
        };
    }
    
    private void MoveToNextPlayer()
    {
        if (Players.Count(p => p.IsStillPlaying()) < 2)
        {
            return;
        }

        var foundNext = false;
        
        var index = CurrentPlayerIndex;
        while (!foundNext)
        {
            index++;
            if (index >= Players.Count)
            {
                index = 0;
            }

            foundNext = Players[index].IsStillPlaying();
        }

        CurrentPlayerIndex = index;
    }

    private static bool CardsHaveSameRank(Card[] cards, out int rank)
    {
        rank = default;
        if (cards.Length == 0)
        {
            return false;
        }
        rank = cards[0].Rank;
        return cards.All(c => c.Rank == cards[0].Rank);
    }

    private bool TryGetCurrentPlayer(Guid playerId, [MaybeNullWhen(false)] out IdiotPlayer player)
    {
        var p = CurrentPlayer;
        if (p.Id != playerId)
        {
            player = default;
            return false;
        }

        player = p;
        return true;
    }
    
    private void IncrementSeed()
    {
        unchecked
        {
            Seed++;
        }
    }
    
    public override Task StartAsync()
    {
        throw new NotImplementedException();
    }
}