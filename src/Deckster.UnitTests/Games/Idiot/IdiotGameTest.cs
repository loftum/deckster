using Deckster.Client.Games.Common;
using Deckster.Server.Collections;
using Deckster.Server.Games;
using Deckster.Server.Games.Idiot;
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
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
        });

        var response = await game.PutCardsFromHand(new PutCardsFromHandRequest{ PlayerId = game.CurrentPlayer.Id, Cards = [new Card(8, Suit.Spades)]});
        Asserts.Success(response);
    }

    [Test]
    public async ValueTask PutCards_Fails_WhenPlayerDoesNotHaveCards()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Steal(8, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
        });

        var response = await game.PutCardsFromHand(new PutCardsFromHandRequest{ PlayerId = game.CurrentPlayer.Id, Cards = [new Card(9, Suit.Spades)]});
        Asserts.Fail(response, "You don't have those cards");
    }

    [Test]
    public async ValueTask PutCards_Fails_WhenNoCards()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
        });

        var response = await game.PutCardsFromHand(new PutCardsFromHandRequest{ PlayerId = game.CurrentPlayer.Id, Cards = [] });
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
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
        });

        var response = await game.PutCardsFromHand(new PutCardsFromHandRequest{ PlayerId = game.CurrentPlayer.Id, Cards = [new Card(8, Suit.Spades), new Card(9, Suit.Spades)]});
        Asserts.Fail(response, "All cards must have same rank");
    }
    
    [Test]
    public async ValueTask PutCards_Fails_WhenRankIsLowerThanTopOfPile()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Steal(8, Suit.Spades));
            g.Players[0].CardsOnHand.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
            
            g.DiscardPile.Push(deck.Steal(10, Suit.Spades));
        });

        var response = await game.PutCardsFromHand(new PutCardsFromHandRequest{ PlayerId = game.CurrentPlayer.Id, Cards = [new Card(8, Suit.Spades)] });
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
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
            
            g.DiscardPile.Push(deck.Steal(12, Suit.Spades));
        });

        Asserts.Success(await game.PutCardsFromHand(new PutCardsFromHandRequest{ PlayerId = game.CurrentPlayer.Id, Cards = [new Card(rank, suit)]}));
    }
    
    [Test]
    public async ValueTask PutTen_FlushesDiscardPile()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Steal(10, Suit.Spades));
            g.Players[0].CardsOnHand.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
        });

        Asserts.Success(await game.PutCardsFromHand(new PutCardsFromHandRequest{ PlayerId = game.CurrentPlayer.Id, Cards = [new Card(10, Suit.Spades)]}));
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
            
            g.Players[0].CardsOnHand.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
        });
        
        Asserts.Success(await game.PutCardsFromHand(new PutCardsFromHandRequest{ PlayerId = game.CurrentPlayer.Id, Cards = cards }));
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
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
        });

        Asserts.Success(await game.PutCardsFromHand(new PutCardsFromHandRequest{ PlayerId = game.CurrentPlayer.Id, Cards = [new Card(rank, suit)] }));
        Assert.That(game.CurrentPlayer, Is.SameAs(game.Players[1]));
    }

    [Test]
    public async ValueTask DrawCards()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.Steal(8, Suit.Spades));
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
            g.StockPile.PushRange(g.Deck);
        });

        Asserts.Success(await game.DrawCards(new DrawCardsRequest { PlayerId = game.CurrentPlayer.Id, NumberOfCards = 2 } ));
        Assert.That(game.CurrentPlayer, Is.SameAs(game.Players[1]));
    }
    
    [Test]
    public async ValueTask DrawCards_Fails_WhenNotYourTurn()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
        });

        Asserts.Fail(await game.DrawCards(new DrawCardsRequest{PlayerId = game.Players[1].Id, NumberOfCards = 1 }), "It is not your turn");
    }
    
    [Test]
    [TestCase(-1, "You have to draw at least 1 card")]
    [TestCase(0, "You have to draw at least 1 card")]
    [TestCase(4, "You can only have 2 more cards on hand")]
    public async ValueTask DrawCards_Fails_WhenNumberOfCardsIsInvalid(int numberOfCards, string expectedError)
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
        });

        Asserts.Fail(await game.DrawCards(new DrawCardsRequest{ PlayerId = game.CurrentPlayer.Id, NumberOfCards = numberOfCards }), expectedError);
    }
    
    [Test]
    public async ValueTask DrawCards_Fails_WhenStockPileIsEmpty()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
        });

        Asserts.Fail(await game.DrawCards(new DrawCardsRequest{ PlayerId = game.CurrentPlayer.Id, NumberOfCards = 2 }), "Not enough cards in stock pile");
    }

    [Test]
    public async ValueTask DrawCards_AfterFlushingDiscardPile_MakesCurrentPlayersPlayAgain()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsOnHand.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
            g.StockPile.PushRange(g.Deck);
            g.DiscardPile.Clear();
            g.LastCardPutBy = g.Players[0].Id;
        });

        Asserts.Success(await game.DrawCards(new DrawCardsRequest{ PlayerId = game.CurrentPlayer.Id, NumberOfCards = 2 }));
        Assert.That(game.CurrentPlayer, Is.SameAs(game.Players[0]));
    }

    [Test]
    public async ValueTask PutCardFacingDown()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsFacingDown.Push(deck.Steal(8, Suit.Spades));
            g.Players[0].CardsFacingDown.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
            g.DiscardPile.Clear();
            g.LastCardPutBy = g.Players[0].Id;
        });

        Asserts.Success(await game.PutCardFacingDown(new PutCardFacingDownRequest{ PlayerId = game.CurrentPlayer.Id, Index = 0 }));
        Assert.That(game.CurrentPlayer, Is.SameAs(game.Players[1]));
    }
    
    [Test]
    public async ValueTask PutCardFacingDown_Fails_WhenNotYourTurn()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsFacingDown.Push(deck.Steal(8, Suit.Spades));
            g.Players[0].CardsFacingDown.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
            g.StockPile.PushRange(deck);
            g.DiscardPile.Clear();
            g.LastCardPutBy = g.Players[0].Id;
        });

        Asserts.Fail(await game.PutCardFacingDown(new PutCardFacingDownRequest{ PlayerId = game.Players[1].Id, Index = 0 }),
            "It is not your turn");
    }

    [Test]
    public async ValueTask PutCardFacingDown_Fails_ForInvalidIndex()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsFacingDown.Push(deck.Steal(8, Suit.Spades));
            g.Players[0].CardsFacingDown.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
            g.DiscardPile.Clear();
            g.LastCardPutBy = g.Players[0].Id;
        });

        Asserts.Success(await game.PutCardFacingDown(new PutCardFacingDownRequest{ PlayerId = game.CurrentPlayer.Id, Index = 0 }));
        Assert.That(game.CurrentPlayer, Is.SameAs(game.Players[1]));
    }
    
    [Test]
    public async ValueTask PutCardFacingDown_Fails_WhenStockPileHasCards()
    {
        var game = SetUpGame(g =>
        {
            var deck = g.Deck;
            g.Players[0].CardsFacingDown.Push(deck.Steal(8, Suit.Spades));
            g.Players[0].CardsFacingDown.Push(deck.StealRandom());
            g.Players[1].CardsOnHand.Push(deck.StealRandom());
            g.Players[2].CardsOnHand.Push(deck.StealRandom());
            g.StockPile.PushRange(deck);
            g.DiscardPile.Clear();
            g.LastCardPutBy = g.Players[0].Id;
        });

        Asserts.Fail(await game.PutCardFacingDown(new PutCardFacingDownRequest{ PlayerId = game.CurrentPlayer.Id, Index = 0 }),
            "There are still cards in stock pile");
    }
   
    
    private static IdiotGame SetUpGame(Action<IdiotGame> configure)
    {
        var game = IdiotGame.Create(new IdiotGameCreatedEvent
        {
            Id = Some.Id,
            Players = Some.FourPlayers(),
            Deck = Decks.Standard
        });

        configure(game);
        
        return game;
    }
}
