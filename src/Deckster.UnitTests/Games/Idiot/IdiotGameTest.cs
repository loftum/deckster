using Deckster.Client.Games.Common;
using Deckster.Server.Collections;
using Deckster.Server.Games.Idiot;
using Deckster.Server.Games.Idiot.Core;
using NUnit.Framework;

namespace Deckster.UnitTests.Games.Idiot;

public class IdiotGameTest
{
    [Test]
    public async ValueTask PutCards()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Get(8, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Get(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Get(8, Suit.Clubs));
        });

        var response = await game.PutCardsFromHand(game.CurrentPlayer.Id, [new Card(8, Suit.Spades)]);
        Asserts.Success(response);
    }

    [Test]
    public async ValueTask PutCards_Fails_WhenPlayerDoesNotHaveCards()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Get(8, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Get(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Get(8, Suit.Clubs));
        });

        var response = await game.PutCardsFromHand(game.CurrentPlayer.Id, [new Card(9, Suit.Spades)]);
        Asserts.Fail(response, "You don't have all of those cards");
    }

    [Test]
    public async ValueTask PutCards_Fails_WhenNoCards()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Get(8, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Get(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Get(8, Suit.Clubs));
        });

        var response = await game.PutCardsFromHand(game.CurrentPlayer.Id, []);
        Asserts.Fail(response, "You must put at least 1 card");
    }

    [Test]
    public async ValueTask PutCards_Fails_WhenCardsHaveDifferentRanks()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Get(8, Suit.Spades));
            g.Players[0].CardsOnHand.Push(deck.Get(9, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Get(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Get(8, Suit.Clubs));
        });

        var response = await game.PutCardsFromHand(game.CurrentPlayer.Id, [new Card(8, Suit.Spades), new Card(9, Suit.Spades)]);
        Asserts.Fail(response, "All cards must have same rank");
    }
    
    private static IdiotGame SetUpGame(Action<IdiotGame> configure)
    {
        var game = IdiotGame.Create(new IdiotGameCreatedEvent
        {
            Id = Some.Id,
            Players = Some.FourPlayers(),
            Deck = TestDeck
        });

        configure(game);
        
        return game;
    }

    private static IdiotGame CreateGame()
    {
        return IdiotGame.Create(new IdiotGameCreatedEvent
        {
            Players = Some.FourPlayers(),
            Deck = TestDeck,
            InitialSeed = Some.Seed 
        });
    }

    private static List<Card> TestDeck => GetCards().ToList();

    // Make sure all players have all suits
    private static IEnumerable<Card> GetCards()
    {
        var ranks = new Dictionary<Suit, int>
        {
            [Suit.Clubs] = 0,
            [Suit.Diamonds] = 0,
            [Suit.Spades] = 0,
            [Suit.Hearts] = 0
        };
        
        while (ranks.Values.Any(v => v < 13))
        {
            foreach (var suit in Enum.GetValues<Suit>())
            {
                for (var ii = 0; ii < 4; ii++)
                {
                    var rank = ranks[suit] + 1;
                    if (rank > 13)
                    {
                        continue;
                    }
                    ranks[suit] = rank;

                    yield return new Card(rank, suit);    
                }
            }
        }
    }
}