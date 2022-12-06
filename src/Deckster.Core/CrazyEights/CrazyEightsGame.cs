using Deckster.Core.Domain;

namespace Deckster.Core.CrazyEights;

public class CrazyEightsGame
{
    public Deck Deck { get; } = Deck.Default;
    public Stack<Card> Pile { get; } = new();
    public List<Player> Players { get; } = new();


    public void AddPlayer(string name)
    {
        var player = new Player
        {
            Name = name
        };
        Players.Add(player);
    }

    public void Start()
    {
        if (Players.Count < 2)
        {
            throw new InvalidOperationException("Too few players");
        }
    }
    
}