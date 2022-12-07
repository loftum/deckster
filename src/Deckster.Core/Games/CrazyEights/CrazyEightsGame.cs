using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Deckster.Core.Collections;
using Deckster.Core.Domain;

namespace Deckster.Core.Games.CrazyEights;

public class CrazyEightsGame
{
    public const int InitialCardsPerPlayer = 5;
    
    private readonly List<CrazyEightsPlayer> _donePlayers = new();
    private int _currentPlayerIndex;
    private int _cardsDrawn;
    
    public Guid Id { get; } = Guid.NewGuid();
    public GameState State => Players.Count(p => p.IsStillPlaying()) > 1 ? GameState.Running : GameState.Finished;
    
    /// <summary>
    /// All the (shuffled) cards in the game
    /// </summary>
    public Deck Deck { get; }

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
    public CrazyEightsPlayer[] Players { get; }

    public Card TopOfPile => DiscardPile.Peek();
    
    
    public CrazyEightsPlayer CurrentPlayer => State == GameState.Finished ? CrazyEightsPlayer.Null : Players[_currentPlayerIndex];

    public CrazyEightsGame(Deck deck, CrazyEightsPlayer[] players)
    {
        Deck = deck;
        Players = players;
        Reset();
    }

    public void Reset()
    {
        foreach (var player in Players)
        {
            player.Cards.Clear();
        }
        
        _currentPlayerIndex = 0;
        _donePlayers.Clear();
        StockPile.Clear();
        StockPile.PushRange(Deck.Cards);
        for (var ii = 0; ii < InitialCardsPerPlayer; ii++)
        {
            foreach (var player in Players)
            {
                player.Cards.Add(StockPile.Pop());
            }
        }
        
        DiscardPile.Clear();
        DiscardPile.Push(StockPile.Pop());
        _donePlayers.Clear();
    }

    public ICommandResult PutCardOnDiscardPile(Guid playerId, Card card)
    {
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            return new FailureResult("It is not your turn");
        }

        if (!player.HasCard(card))
        {
            return new FailureResult($"You don't have '{card}'");
        }

        if (!CanPut(card))
        {
            return new FailureResult($"Cannot put '{card}' on '{TopOfPile}'");
        }
        
        player.Cards.Remove(card);
        DiscardPile.Push(card);
        if (!player.Cards.Any())
        {
            _donePlayers.Add(player);
        }
        MoveToNextPlayer();
        
        return GetPlayerViewOfGame(player);
    }
    
    public ICommandResult DrawCardFromStockPile(Guid playerId)
    {
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            return new FailureResult("It is not your turn");
        }
        
        ShufflePileIfNecessary();
        if (!StockPile.Any())
        {
            return new FailureResult("No more cards");
        }
        var card = StockPile.Pop();
        player.Cards.Add(card);
        _cardsDrawn++;
        if (_cardsDrawn > 2)
        {
            MoveToNextPlayer();
            _cardsDrawn = 0;
        }
        return new CardResult(card);
    }
    
    public ICommandResult Pass(Guid playerId)
    {
        if (!TryGetCurrentPlayer(playerId, out _))
        {
            return new FailureResult("It is not your turn");
        }
        
        MoveToNextPlayer();
        return new SuccessResult();
    }

    private PlayerViewOfGame GetPlayerViewOfGame(CrazyEightsPlayer player)
    {
        return new PlayerViewOfGame
        {
            Cards = player.Cards,
            TopOfPile = TopOfPile,
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
            if (index >= Players.Length)
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
        return TopOfPile.Suit == card.Suit ||
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
        var reshuffledCards = DiscardPile.ToList().KnuthShuffle();
        DiscardPile.Clear();
        DiscardPile.Push(topOfPile);
        StockPile.PushRange(reshuffledCards);
    }

    public ICommandResult GetStateFor(Guid userId)
    {
        var player = Players.FirstOrDefault(p => p.Id == userId);
        if (player == null)
        {
            return new FailureResult($"There is no player '{userId}'");
        }

        return GetPlayerViewOfGame(player);
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