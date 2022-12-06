using Deckster.Core.Collections;
using Deckster.Core.Domain;

namespace Deckster.Core.CrazyEights;

public class CrazyEightsGame
{
    public const int MaxNumberOfPlayers = 4;
    public const int InitialCardsPerPlayer = 5;
    
    public GameState State { get; private set; }
    public Deck Deck { get; } = Deck.Default.Shuffle();

    public Stack<Card> Pile { get; } = new();
    public Stack<Card> Pack { get; } = new();
    public List<Player> Players { get; } = new();
    public Player CurrentPlayer { get; private set; } = Player.Null;

    public void AddPlayer(string name)
    {
        var player = new Player
        {
            Name = name
        };
        if (Players.Count >= MaxNumberOfPlayers)
        {
            throw new InvalidOperationException("Max players exceeded");
        }
        Players.Add(player);
    }

    public void Start()
    {
        if (Players.Count < 2)
        {
            throw new InvalidOperationException("Too few players");
        }

        Pack.Clear();
        Pack.PushRange(Deck.Cards);

        for (var ii = 0; ii < InitialCardsPerPlayer; ii++)
        {
            foreach (var player in Players)
            {
                player.Cards.Add(Pack.Pop());
            }
        }

        Pile.Clear();
        Pile.Push(Pack.Pop());

        CurrentPlayer = Players.Random();

        State = GameState.Running;
    }

    public void PutCardOnPile(Guid playerId, Guid cardId)
    {
        AssertGameIsRunning();
        if (CurrentPlayer.Id != playerId)
        {
            throw new InvalidOperationException($"It is not '{playerId}''s turn.");
        }

        var card = CurrentPlayer.Cards.FirstOrDefault(c => c.Id == cardId);
        if (card == null)
        {
            throw new InvalidOperationException($"Invalid card '{cardId}'");
        }

        if (!CanPut(card, Pile.Peek()))
        {
            throw new InvalidOperationException($"Can not put '{card}' on '{Pile.Peek()}'");
        }

        CurrentPlayer.Cards.Remove(card);
        
        Pile.Push(card);
    }

    private static bool CanPut(Card card, Card pileCard)
    {
        return pileCard.Suit == card.Suit ||
               pileCard.Rank == card.Rank ||
               card.Rank == 8;
    }

    private void AssertGameIsRunning()
    {
        if (State == GameState.Waiting)
        {
            throw new InvalidOperationException("Game is not running");
        }
    }
}