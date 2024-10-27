using Deckster.Client.Games.Common;
using Deckster.Client.Games.Uno;
using Deckster.Server.Collections;
using Deckster.Server.Games.Uno.Core;
using NUnit.Framework;

namespace Deckster.UnitTests.Games;

public class UnoGameTest
{
    [Test]
    public async ValueTask PutCard_Succeeds()
    {
        var game = SetUpGame(g =>
        {
            var cards = g.Deck;
            g.CurrentPlayer.Cards.Add(cards.Get(UnoValue.Eight, UnoColor.Blue));
            g.DiscardPile.Push(cards.Get(UnoValue.Seven, UnoColor.Blue));
        });
        
        var card = new UnoCard(UnoValue.Eight, UnoColor.Blue);
        var result = await game.PutCard(game.CurrentPlayer.Id, card);
        Asserts.Success(result);
    }

    [Test]
    public async ValueTask PutCard_Fails_WhenNotYourTurn()
    {
        var game = CreateGame();
        var player = game.Players[1];
        var result = await game.PutCard(player.Id, player.Cards[0]);

        Asserts.Fail(result, "It is not your turn");
    }
    
    [Test]
    [TestCase(UnoValue.One, UnoColor.Red, "You don't have 'One Red'")]
    [TestCase(UnoValue.Seven, UnoColor.Blue, "Cannot put 'Seven Blue' on 'One Green'")]
    public async Task PutCard_Fails(UnoValue value, UnoColor color, string errorMessage)
    {
        var game = SetUpGame(g =>
        {
            var cards = g.Deck;
            g.Players[0].Cards.Add(cards.Get(UnoValue.Eight, UnoColor.Blue));
            g.Players[0].Cards.Add(cards.Get(UnoValue.Seven, UnoColor.Blue));
            
            g.Players[1].Cards.Add(cards.Get(UnoValue.Eight, UnoColor.Red));
            
            g.DiscardPile.Push(cards.Get(UnoValue.One, UnoColor.Green));
        });
        var player = game.Players[0];
        var card = new UnoCard(value, color);
        
        var result = await game.PutCard(player.Id, card);
        Asserts.Fail(result, errorMessage);
    }
    
    private static UnoGame SetUpGame(Action<UnoGame> configure)
    {
        var players = new List<PlayerData>
        {
            new()
            {
                Id = Some.Id,
                Name = Some.PlayerName
            },
            new()
            {
                Id = Some.OtherId,
                Name = Some.OtherPlayerName
            },
            new()
            {
                Id = Some.YetAnotherId,
                Name = Some.YetAnotherPlayerName
            },
            new()
            {
                Id = Some.TotallyDifferentId,
                Name = Some.TotallyDifferentPlayerName
            }
        };

        var game = UnoGame.Create(new UnoGameCreatedEvent
        {
            Id = Some.Id,
            Players = players,
            Deck = TestDeck
        });

        configure(game);
        
        return game;
    }

    private static UnoGame CreateGame()
    {
        return UnoGame.Create(new UnoGameCreatedEvent
        {
            Players =
            [
                new()
                {
                    Id = Some.Id,
                    Name = Some.PlayerName
                },

                new()
                {
                    Id = Some.OtherId,
                    Name = Some.OtherPlayerName
                },

                new()
                {
                    Id = Some.YetAnotherId,
                    Name = Some.YetAnotherPlayerName
                },

                new()
                {
                    Id = Some.TotallyDifferentId,
                    Name = Some.TotallyDifferentPlayerName
                }
            ],
            Deck = TestDeck,
            InitialSeed = Some.Seed 
        });
    }

    private static List<UnoCard> TestDeck => UnoDeck.Standard;
}