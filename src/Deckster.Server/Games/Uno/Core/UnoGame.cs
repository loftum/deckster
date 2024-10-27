
using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Games.Common;
using Deckster.Client.Games.Uno;
using Deckster.Server.Collections;
using Deckster.Server.Games.Common;
using Deckster.Server.Games.CrazyEights.Core;

namespace Deckster.Server.Games.Uno.Core;

public class UnoGameCreatedEvent : GameCreatedEvent
{
    public List<PlayerData> Players { get; init; } = [];
    public List<UnoCard> Deck { get; init; } = [];
}

public class UnoGame : GameObject
{
    private readonly int _initialCardsPerPlayer = 7;

    public int CurrentPlayerIndex { get; set; }
    public int CardsDrawn { get; set; }
    public int GameDirection {get; set;} = 1;

    public override GameState State => Players.Count(p => p.IsStillPlaying()) > 1 ? GameState.Running : GameState.Finished;

    public int Seed { get; set; }
    
    /// <summary>
    /// All the (shuffled) cards in the game
    /// </summary>
    public List<UnoCard> Deck { get; init; } = [];

    /// <summary>
    /// Where players draw cards from
    /// </summary>
    public List<UnoCard> StockPile { get; } = new();
    
    /// <summary>
    /// Where players put cards
    /// </summary>
    public List<UnoCard> DiscardPile { get; } = new();

    /// <summary>
    /// All the players
    /// </summary>
    public List<UnoPlayer> Players { get; init; } = [];
 
    public UnoColor? NewColor { get; set; }
    public UnoCard TopOfPile => DiscardPile.Peek();
    public UnoColor CurrentColor => NewColor ?? TopOfPile.Color;
    
    public UnoPlayer CurrentPlayer => State == GameState.Finished ? UnoPlayer.Null : Players[CurrentPlayerIndex];

    public static UnoGame Create(UnoGameCreatedEvent created)
    {
        var game = new UnoGame
        {
            Id = created.Id,
            StartedTime = created.StartedTime,
            Players = created.Players.Select(p => new UnoPlayer
            {
                Id = p.Id,
                Name = p.Name
            }).ToList(),
            Deck = created.Deck,
            Seed = created.InitialSeed
        };

        return game;
    }

    public override async Task StartAsync()
    {
        foreach (var player in Players)
        {
            await Communication.NotifyAsync(player.Id, new GameStartedNotification
            {
                GameId = Id,
                PlayerViewOfGame = GetPlayerViewOfGame(player)
            });
        }
        
        await Communication.NotifyAsync(CurrentPlayer.Id, new ItsYourTurnNotification
        {
            PlayerViewOfGame = GetPlayerViewOfGame(CurrentPlayer)
        });
    }

    public void ScoreRound(UnoPlayer winner)
    {
        winner.Score += Players.Where(x => x.Id != winner.Id).Sum(p => p.CalculateHandScore());
    }
    
    public void NewRound(DateTimeOffset operationTime)
    {
        foreach (var player in Players)
        {
            player.Cards.Clear();
        }
        
        CurrentPlayerIndex = 0;
        StockPile.Clear();
        StockPile.PushRange(Deck);
        for (var ii = 0; ii < _initialCardsPerPlayer; ii++)
        {
            foreach (var player in Players)
            {
                player.Cards.Add(StockPile.Pop());
            }
        }
        
        DiscardPile.Clear();
        DiscardPile.Push(StockPile.Pop());
    }
    
    public UnoResponse PutCard(Guid playerId, UnoCard card)
    {
        IncrementSeed();
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            return new UnoFailureResponse("It is not your turn");
        }

        if (!player.HasCard(card))
        {
            return new UnoFailureResponse($"You don't have '{card}'");
        }

        if (!CanPut(card))
        {
            return new UnoFailureResponse($"Cannot put '{card}' on '{TopOfPile}'");
        }
        
        if(CardsDrawn < 0)
        {
            return new UnoFailureResponse($"You have to draw {CardsDrawn*-1} cards");
        }
        
        player.Cards.Remove(card);
        DiscardPile.Push(card);
        NewColor = null;
        if (!player.Cards.Any())
        {
            ScoreRound(player);
            NewRound(DateTimeOffset.UtcNow);
            return new UnoSuccessResponse();
        }

        if(card.Value == UnoValue.DrawTwo)
        {
            CardsDrawn = -2;
        }
        else if(card.Value == UnoValue.Reverse)
        {
            GameDirection *= -1;
        }
        else if(card.Value == UnoValue.Skip)
        {
            MoveToNextPlayer();
        }
        else if(card.Value == UnoValue.WildDrawFour)
        {
            CardsDrawn = -4;
        }
        MoveToNextPlayer();
        
        return GetPlayerViewOfGame(player);
    }
    
    public UnoResponse PutWild(Guid playerId, UnoCard card, UnoColor newColor)
    {
        IncrementSeed();
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            return new UnoFailureResponse("It is not your turn");
        }

        if (!player.HasCard(card))
        {
            return new UnoFailureResponse($"You don't have '{card}'");
        }
        
        if (card.Color != UnoColor.Wild)
        {
            return new UnoFailureResponse("Card color must be 'Wild'");
        }

        if(newColor == UnoColor.Wild)
        {
            return new UnoFailureResponse("New color cannot be 'Wild'");
        }
        
        if (!CanPut(card))
        {
            return NewColor.HasValue
                ? new UnoFailureResponse($"Cannot put '{card}' on '{TopOfPile}' (new suit: '{NewColor.Value}')")
                : new UnoFailureResponse($"Cannot put '{card}' on '{TopOfPile}'");
        }

        player.Cards.Remove(card);
        DiscardPile.Push(card);
        NewColor = newColor;
        if (!player.Cards.Any())
        {
            ScoreRound(player);
            NewRound(DateTimeOffset.UtcNow);
            return new UnoSuccessResponse();
        }

        MoveToNextPlayer();
        
        return GetPlayerViewOfGame(player);
    }
    
    
    public UnoResponse DrawCard(Guid playerId)
    {
        IncrementSeed();
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            return new UnoFailureResponse("It is not your turn");
        }
  
        if (CardsDrawn == 1)
        {
            return new UnoFailureResponse("You can only draw 1 card, then pass if you can't play");
        }
        
        ShufflePileIfNecessary();
        if (!StockPile.Any())
        {
            return new UnoFailureResponse("No more cards");
        }
        var card = StockPile.Pop();
        player.Cards.Add(card);
        CardsDrawn++;
        if (CardsDrawn == 0) //we just paid the last penalty. Now we skip our turn
        {
            MoveToNextPlayer();
        }
        return new UnoCardsResponse(card);
    }
    
    public UnoResponse Pass(Guid playerId)
    {
        IncrementSeed();
        if (!TryGetCurrentPlayer(playerId, out _))
        {
            return new UnoFailureResponse("It is not your turn");
        }

        if (CardsDrawn != 1)
        {
            return new UnoFailureResponse("You have to draw a card first");
        }
        
        MoveToNextPlayer();
        return new UnoSuccessResponse();
    }
    
    private PlayerViewOfUnoGame GetPlayerViewOfGame(UnoPlayer player)
    {
        return new PlayerViewOfUnoGame
        {
            Cards = player.Cards,
            TopOfPile = TopOfPile,
            CurrentSuit = CurrentColor,
            DiscardPileCount = DiscardPile.Count,
            StockPileCount = StockPile.Count,
            OtherPlayers = Players.Where(p => p.Id != player.Id).Select(ToOtherPlayer).ToList()
        };
    }
    
    private bool TryGetCurrentPlayer(Guid playerId, [MaybeNullWhen(false)] out UnoPlayer player)
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
            index+=GameDirection;
            if (index >= Players.Count)
            {
                index = 0;
            }

            if (index < 0)
            {
                index = Players.Count - 1;
            }
            foundNext = Players[index].IsStillPlaying();
        }

        CurrentPlayerIndex = index;
        CardsDrawn = 0;
    }

    private bool CanPut(UnoCard card)
    {
        return CurrentColor == card.Color ||
               TopOfPile.Value == card.Value ||
               card.Color == UnoColor.Wild;
    }
    
    private void ShufflePileIfNecessary()
    {
        if (StockPile.Any())
        {
            return;
        }
        
        if (DiscardPile.Count < 2)
        {
            return;
        }

        var topOfPile = DiscardPile.Pop();
        var reshuffledCards = DiscardPile.KnuthShuffle(Seed);
        DiscardPile.Clear();
        DiscardPile.Push(topOfPile);
        StockPile.PushRange(reshuffledCards);
    }

    private static OtherUnoPlayer ToOtherPlayer(UnoPlayer player)
    {
        return new OtherUnoPlayer
        {
            Name = player.Name,
            NumberOfCards = player.Cards.Count
        };
    }

    private void IncrementSeed()
    {
        unchecked
        {
            Seed++;
        }
    }
}