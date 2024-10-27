using Deckster.Client.Games.Common;
using Deckster.Server.Games.Common;

namespace Deckster.Server.Games.Idiot.Core;

public class IdiotGame : GameObject
{
    public int Seed { get; set; }
    public override GameState State => Players.Count(p => p.IsStillPlaying()) > 1 ? GameState.Running : GameState.Finished;
    
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
    
    public override Task StartAsync()
    {
        throw new NotImplementedException();
    }
}

public class IdiotPlayer
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "";
    public List<Card> CardsOnHand { get; init; } = [];
    public List<Card> VisibleTableCards { get; init; } = [];
    public List<Card> HiddenTableCards { get; init; } = [];

    public bool IsStillPlaying() => CardsOnHand.Any() || VisibleTableCards.Any() || HiddenTableCards.Any();
    
    public static readonly IdiotPlayer Null = new()
    {
        Id = Guid.Empty,
        Name = "Ing. Kognito"
    };
}