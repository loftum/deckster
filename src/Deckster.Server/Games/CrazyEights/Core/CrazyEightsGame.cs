using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Common;
using Deckster.Client.Games.Common;
using Deckster.Client.Games.CrazyEights;
using Deckster.Server.Data;
using Deckster.Server.Games.Common;

namespace Deckster.Server.Games.CrazyEights.Core;

public class CrazyEightsGameStartedEvent
{
    public int Seed { get; init; } = DateTimeOffset.UtcNow.Nanosecond;
    public List<PlayerData> Players { get; init; } = [];
    public List<Card> Deck { get; init; } = [];
}

public class CrazyEightsGame : DatabaseObject
{
    public int Version { get; set; }
    public int Seed { get; set; }
    
    private readonly int _initialCardsPerPlayer = 5;
    
    public List<CrazyEightsPlayer> DonePlayers { get; } = new();
    private int _currentPlayerIndex;
    private int _cardsDrawn;
    
    public GameState State => Players.Count(p => p.IsStillPlaying()) > 1 ? GameState.Running : GameState.Finished;
    
    /// <summary>
    /// All the (shuffled) cards in the game
    /// </summary>
    public List<Card> Deck { get; set; }

    /// <summary>
    /// Where players draw cards from
    /// </summary>
    public Stack<Card> StockPile { get; } = new();
    
    /// <summary>
    /// Where players put cards
    /// </summary>
    public Stack<Card> DiscardPile { get; } = new();

    /// <summary>
    /// All the players
    /// </summary>
    public List<CrazyEightsPlayer> Players { get; init; } = [];

    private Suit? _newSuit;
    public Card TopOfPile => DiscardPile.Peek();
    public Suit CurrentSuit => _newSuit ?? TopOfPile.Suit;

    public CrazyEightsPlayer CurrentPlayer => State == GameState.Finished ? CrazyEightsPlayer.Null : Players[_currentPlayerIndex];

    public static CrazyEightsGame Create(CrazyEightsGameStartedEvent started)
    {
        var game = new CrazyEightsGame
        {
            Players = started.Players.Select(p => new CrazyEightsPlayer
            {
                Id = p.Id,
                Name = p.Name
            }).ToList(),
            Deck = started.Deck
        };

        return game;
    }
    
    public void Reset()
    {
        foreach (var player in Players)
        {
            player.Cards.Clear();
        }
        
        _currentPlayerIndex = 0;
        DonePlayers.Clear();
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
        DonePlayers.Clear();
    }

    public void Apply(PutCardRequest request, out CrazyEightsResponse response)
    {
        response = PutCard(request.PlayerId, request.Card);
    }
    
    public void Apply(PutEightRequest request, out CrazyEightsResponse response)
    {
        response = PutEight(request.PlayerId, request.Card, request.NewSuit);
    }
    
    public void Apply(DrawCardRequest request, out CrazyEightsResponse response)
    {
        response = DrawCard(request.PlayerId);
    }
    
    public void Apply(PassRequest request, out CrazyEightsResponse response)
    {
        response = Pass(request.PlayerId);
    }

    private void IncrementSeed()
    {
        unchecked
        {
            Seed++;
        }
    }

    public CrazyEightsResponse PutCard(Guid playerId, Card card)
    {
        IncrementSeed();
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            return new CrazyEightsFailureResponse("It is not your turn");
        }

        if (!player.HasCard(card))
        {
            return new CrazyEightsFailureResponse($"You don't have '{card}'");
        }

        if (!CanPut(card))
        {
            return new CrazyEightsFailureResponse($"Cannot put '{card}' on '{TopOfPile}'");
        }
        
        player.Cards.Remove(card);
        DiscardPile.Push(card);
        _newSuit = null;
        if (!player.Cards.Any())
        {
            DonePlayers.Add(player);
        }

        MoveToNextPlayer();
        
        return GetPlayerViewOfGame(player);
    }

    public CrazyEightsResponse PutEight(Guid playerId, Card card, Suit newSuit)
    {
        IncrementSeed();
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            return new CrazyEightsFailureResponse("It is not your turn");
        }

        if (!player.HasCard(card))
        {
            return new CrazyEightsFailureResponse($"You don't have '{card}'");
        }
        
        if (card.Rank != 8)
        {
            return new CrazyEightsFailureResponse("Card rank must be '8'");
        }

        if (!CanPut(card))
        {
            return _newSuit.HasValue
                ? new CrazyEightsFailureResponse($"Cannot put '{card}' on '{TopOfPile}' (new suit: '{_newSuit.Value}')")
                : new CrazyEightsFailureResponse($"Cannot put '{card}' on '{TopOfPile}'");
        }

        player.Cards.Remove(card);
        DiscardPile.Push(card);
        _newSuit = newSuit != card.Suit ? newSuit : null;
        if (!player.Cards.Any())
        {
            DonePlayers.Add(player);
        }

        MoveToNextPlayer();
        
        return GetPlayerViewOfGame(player);
    }
    
    public CrazyEightsResponse DrawCard(Guid playerId)
    {
        IncrementSeed();
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            return new CrazyEightsFailureResponse("It is not your turn");
        }
        
        if (_cardsDrawn > 2)
        {
            return new CrazyEightsFailureResponse("You can only draw 3 cards");
        }
        
        ShufflePileIfNecessary();
        if (!StockPile.Any())
        {
            return new CrazyEightsFailureResponse("Stock pile is empty");
        }
        var card = StockPile.Pop();
        player.Cards.Add(card);
        _cardsDrawn++;
        
        return new CardResponse(card);
    }
    
    public CrazyEightsResponse Pass(Guid playerId)
    {
        IncrementSeed();
        if (!TryGetCurrentPlayer(playerId, out _))
        {
            return new CrazyEightsFailureResponse("It is not your turn");
        }
        
        MoveToNextPlayer();
        return new CrazyEightsSuccessResponse();
    }

    private PlayerViewOfGame GetPlayerViewOfGame(CrazyEightsPlayer player)
    {
        return new PlayerViewOfGame
        {
            Cards = player.Cards,
            TopOfPile = TopOfPile,
            CurrentSuit = CurrentSuit,
            DiscardPileCount = DiscardPile.Count,
            StockPileCount = StockPile.Count,
            OtherPlayers = Players.Where(p => p.Id != player.Id).Select(ToOtherPlayer).ToList()
        };
    }

    private bool TryGetCurrentPlayer(Guid playerId, [MaybeNullWhen(false)] out CrazyEightsPlayer player)
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
        
        var index = _currentPlayerIndex;
        while (!foundNext)
        {
            index++;
            if (index >= Players.Count)
            {
                index = 0;
            }

            foundNext = Players[index].IsStillPlaying();
        }

        _currentPlayerIndex = index;
        _cardsDrawn = 0;
    }

    private bool CanPut(Card card)
    {
        return CurrentSuit == card.Suit ||
               TopOfPile.Rank == card.Rank ||
               card.Rank == 8;
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
        var reshuffledCards = DiscardPile.ToList().KnuthShuffle(Seed);
        DiscardPile.Clear();
        DiscardPile.Push(topOfPile);
        StockPile.PushRange(reshuffledCards);
    }

    private static OtherCrazyEightsPlayer ToOtherPlayer(CrazyEightsPlayer player)
    {
        return new OtherCrazyEightsPlayer
        {
            Name = player.Name,
            NumberOfCards = player.Cards.Count
        };
    }
}