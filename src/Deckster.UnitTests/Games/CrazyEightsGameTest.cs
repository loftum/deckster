using System.Text.Json;
using System.Text.Json.Serialization;
using Deckster.Client.Common;
using Deckster.Client.Games.Common;
using Deckster.Server.Games.CrazyEights;
using NUnit.Framework;

namespace Deckster.UnitTests.Games;

[TestFixture]
public class CrazyEightsGameTest
{
    [Test]
    public void Print()
    {
        var game = CreateGame();
        Console.WriteLine(JsonSerializer.Serialize(game.Players[0], new JsonSerializerOptions{WriteIndented = true, Converters = { new JsonStringEnumConverter() }}));
        Console.WriteLine(game.TopOfPile);
    }

    [Test]
    [TestCase(9, Suit.Diamonds)]
    [TestCase(8, Suit.Hearts)]
    public void PutCard_Succeeds(int rank, Suit suit)
    {
        var game = CreateGame();
        var player = game.Players[0];
        var card = new Card(rank, suit);
        var result = game.PutCard(player.Id, card);
        AssertSuccess(result);
    }
    
    [Test]
    public void PutCard_Fails_WhenNotYourTurn()
    {
        var game = CreateGame();
        var player = game.Players[1];
        var result = game.PutCard(player.Id, player.Cards[0]);
        
        AssertFail(result, "It is not your turn");
    }

    [Test]
    [TestCase(1, Suit.Clubs, "You don't have 'A♧'")]
    [TestCase(12, Suit.Diamonds, "Cannot put 'Q♢' on '10♧'")]
    public void PutCard_Fails(int rank, Suit suit, string errorMessage)
    {
        var game = CreateGame();
        var player = game.Players[0];
        var card = new Card(rank, suit);
        
        var result = game.PutCard(player.Id, card);
        AssertFail(result, errorMessage);
    }

    [Test]
    public void DrawCard_Fails_AfterThreeAttempts()
    {
        var game = CreateGame();
        var player = game.Players[0];

        for (var ii = 0; ii < 3; ii++)
        {
            game.DrawCard(player.Id);
        }
        
        var result = game.DrawCard(player.Id);
        AssertFail(result, "You can only draw 3 cards");
    }

    [Test]
    public void Pass_SucceedsAlways()
    {
        var game = CreateGame();
        var player = game.Players[0];
        var result = game.Pass(player.Id);
        AssertSuccess(result);
    }

    [Test]
    [TestCase(Suit.Clubs)]
    [TestCase(Suit.Diamonds)]
    [TestCase(Suit.Spades)]
    [TestCase(Suit.Hearts)]
    public void PutEight_ChangesSuit(Suit newSuit)
    {
        var game = CreateGame();
        var player = game.Players[0];
        var eight = new Card(8, Suit.Hearts);
        AssertSuccess(game.PutEight(player.Id, eight, newSuit));
        Assert.That(game.CurrentSuit, Is.EqualTo(newSuit));

        var nextPlayer = game.CurrentPlayer;
        var cardWithNewSuit = nextPlayer.Cards.First(c => c.Suit == newSuit && c.Rank != 8);
        
        AssertSuccess(game.PutCard(nextPlayer.Id, cardWithNewSuit));
    }

    [Test]
    public void PutEight_Fails_WhenNotEight()
    {
        var game = CreateGame();
        var player = game.Players[0];
        var notEight = player.Cards[0];
        var result = game.PutEight(player.Id, notEight, Suit.Clubs); 
        AssertFail(result, "Card rank must be '8'");
    }

    private static void AssertSuccess(CommandResult result)
    {
        switch (result)
        {
            case SuccessResult:
                break;
            case FailureResult r:
                Assert.Fail($"Expeced success, but got '{r.Message}'");
                break;
        }
    }

    private static void AssertFail(CommandResult result, string message)
    {
        switch (result)
        {
            case SuccessResult:
                Assert.Fail("Expected failure, but got success");
                break;
            case FailureResult r:
                Assert.That(r.Message, Is.EqualTo(message));
                break;
        }
    }

    private static CrazyEightsGame CreateGame()
    {
        var players = new[]
        {
            new CrazyEightsPlayer
            {
                Id = Some.Id,
                Name = Some.PlayerName
            },
            new CrazyEightsPlayer
            {
                Id = Some.OtherId,
                Name = Some.OtherPlayerName
            },
            new CrazyEightsPlayer
            {
                Id = Some.AnotherId,
                Name = Some.AnotherPlayerName
            }
        };
        var game = new CrazyEightsGame(Deck.Standard, players, 10);
        return game;
    }
}