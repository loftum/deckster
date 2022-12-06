using Deckster.Core.CrazyEights;
using NUnit.Framework;

namespace Deckster.UnitTests;

[TestFixture]
public class CrazyEightGameTest
{
    [Test]
    public void StartGame_TooFewPlayers_Fails()
    {
        var game = new CrazyEightsGame();
        game.AddPlayer(Some.PlayerName);
        Assert.That(game.Start, Throws.Exception);
    }

    [Test]
    public void Something()
    {
        var game = new CrazyEightsGame();
        game.AddPlayer(Some.PlayerName);
        game.AddPlayer(Some.OtherPlayerName);
        game.Start();
    }
    
}

public static class Some
{
    public const string PlayerName = "Kamuf Larsen";
    public const string OtherPlayerName = "Ellef van Znabel";
    public const string AnotherPlayerName = "Sølvi Normalbakken";
}