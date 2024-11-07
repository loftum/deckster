using System.Diagnostics.CodeAnalysis;
using Deckster.Core.Games.Common;
using Deckster.Games.Collections;

namespace Deckster.Games.Yaniv;

public class YanivGame : GameObject
{
    public override GameState State { get; }
    public List<YanivPlayer> Players { get; init; } = [];
    public List<Card> Deck { get; init; } = [];
    public List<Card> StockPile { get; init; } = [];
    public List<Card> DiscardPile { get; init; } = [];
    public Card? TopOfPile => DiscardPile.Peek();
    
    public YanivPlayer CurrentPlayer => State == GameState.Finished ? YanivPlayer.Null : Players[CurrentPlayerIndex];
    public int CurrentPlayerIndex { get; set; }

    public static YanivGame Create(YanivGameCreatedEvent created)
    {
        var game = new YanivGame
        {
            Id = created.Id,
            StartedTime = created.StartedTime,
            Seed = created.InitialSeed,
            Deck = created.Deck,
        };
        
        return game;
    }
    
    public override Task StartAsync()
    {
        throw new NotImplementedException();
    }

    public void Deal()
    {
        StockPile.Clear();
        DiscardPile.Clear();
        StockPile.PushRange(Deck);
        
        for (var ii = 0; ii < 5; ii++)
        {
            foreach (var player in Players)
            {
                player.CardsOnHand.Add(StockPile.Pop());
            }
        }
        CurrentPlayerIndex = new Random(Seed).Next(0, Players.Count);
    }

    public async Task<PutCardsResponse> PutCards(PutCardsRequest request)
    {
        IncrementSeed();
        var playerId = request.PlayerId;
        PutCardsResponse response;
        
        if (!TryGetCurrentPlayer(playerId, out var player))
        {
            response = new PutCardsResponse {Error = "It is not your turn"};
            await RespondAsync(playerId, response);
            return response;
        }
        
        if (!player.HasCards(request.Cards))
        {
            response = new PutCardsResponse { Error = "You don't have those cards" };
            await RespondAsync(playerId, response);
            return response;
        }

        if (!CanPlay(request.Cards, out var error))
        {
            response = new PutCardsResponse { Error = error };
            await RespondAsync(playerId, response);
            return response;
        }
    }

    private bool CanPlay(Card[] cards, [MaybeNullWhen(true)] out string error)
    {
        error = default;
        switch (cards.Length)
        {
            case 0:
                error = "You must play at least 1 card";
                return false;
            case 1:
                return true;
            case 2:
                if (cards.Any(c => c.IsJoker()) || cards[0].Rank == cards[1].Rank)
                {
                    return true;
                }
                error = "Both cards must be of same rank";
                return false;
            default:

                var previousRank = -1;
                foreach (var card in cards.OrderBy(c => c.Rank))
                {
                    if (previousRank == -1)
                    {
                        previousRank = card.Rank;
                        continue;
                    }
                }
                
                var rank = cards[0].Rank;
                if (cards.Where(c => !c.IsJoker()).All(c => ))
                return cards.All(c => c.Rank == rank);
        }
    }

    private bool TryGetCurrentPlayer(Guid playerId, [MaybeNullWhen(false)] out YanivPlayer player)
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
}