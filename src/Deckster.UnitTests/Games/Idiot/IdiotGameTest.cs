using System.Net.Security;
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
            g.Players[0].CardsOnHand.Push(deck.Steal(8, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Steal(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Steal(8, Suit.Clubs));
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
            g.Players[0].CardsOnHand.Push(deck.Steal(8, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Steal(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Steal(8, Suit.Clubs));
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
            g.Players[0].CardsOnHand.Push(deck.Steal(8, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Steal(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Steal(8, Suit.Clubs));
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
            g.Players[0].CardsOnHand.Push(deck.Steal(8, Suit.Spades));
            g.Players[0].CardsOnHand.Push(deck.Steal(9, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Steal(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Steal(8, Suit.Clubs));
        });

        var response = await game.PutCardsFromHand(game.CurrentPlayer.Id, [new Card(8, Suit.Spades), new Card(9, Suit.Spades)]);
        Asserts.Fail(response, "All cards must have same rank");
    }
    
    [Test]
    public async ValueTask PutCards_Fails_WhenRankIsLowerThanTopOfPile()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Steal(8, Suit.Spades));
            g.Players[0].CardsOnHand.Push(deck.Steal(9, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Steal(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Steal(8, Suit.Clubs));
            
            g.DiscardPile.Push(deck.Steal(10, Suit.Spades));
        });

        var response = await game.PutCardsFromHand(game.CurrentPlayer.Id, [new Card(8, Suit.Spades)]);
        Asserts.Fail(response, "Rank (8) must be equal to or higher than current rank (10)");
    }
    
    [Test]
    [TestCase(10, Suit.Spades)]
    [TestCase(2, Suit.Spades)]
    public async ValueTask PutSpecialCard_AlwaysWorks(int rank, Suit suit)
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Steal(10, Suit.Spades));
            g.Players[0].CardsOnHand.Push(deck.Steal(2, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Steal(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Steal(8, Suit.Clubs));
            
            g.DiscardPile.Push(deck.Steal(12, Suit.Spades));
        });

        Asserts.Success(await game.PutCardsFromHand(game.CurrentPlayer.Id, [new Card(rank, suit)]));
    }
    
    [Test]
    public async ValueTask PutTen_FlushesDiscardPile()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Steal(10, Suit.Spades));
            g.Players[0].CardsOnHand.Push(deck.Steal(9, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Steal(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Steal(8, Suit.Clubs));
        });

        Asserts.Success(await game.PutCardsFromHand(game.CurrentPlayer.Id, [new Card(10, Suit.Spades)]));
        Assert.That(game.DiscardPile, Is.Empty);
        Assert.That(game.CurrentPlayer, Is.SameAs(game.Players[0]));
    }
    
    [Test]
    public async ValueTask PutFourOfSameRank_FlushesDiscardPile()
    {
        Card[] cards = [new(7, Suit.Spades), new(7, Suit.Clubs), new(7, Suit.Hearts), new(7, Suit.Diamonds)];
        
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            foreach (var card in cards)
            {
                g.Players[0].CardsOnHand.Push(deck.Steal(card));    
            }
            
            g.Players[0].CardsOnHand.Push(deck.Steal(9, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.Steal(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Steal(8, Suit.Clubs));
        });
        
        Asserts.Success(await game.PutCardsFromHand(game.CurrentPlayer.Id, cards));
        Assert.That(game.DiscardPile, Is.Empty);
        
        
        Assert.That(game.CurrentPlayer, Is.SameAs(game.Players[0]));
    }

    [Test]
    [TestCase(8, Suit.Spades)]
    [TestCase(2, Suit.Spades)]
    [TestCase(10, Suit.Spades)]
    public async ValueTask PutLastCard_MovesToNextPlayer_RegardlessOfCard(int rank, Suit suit)
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Steal(rank, suit));
            g.Players[1].CardsOnHand.Push(deck.Steal(8, Suit.Diamonds));
            g.Players[2].CardsOnHand.Push(deck.Steal(8, Suit.Clubs));
        });

        Asserts.Success(await game.PutCardsFromHand(game.CurrentPlayer.Id, [new Card(rank, suit)]));
        Assert.That(game.CurrentPlayer, Is.SameAs(game.Players[1]));
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