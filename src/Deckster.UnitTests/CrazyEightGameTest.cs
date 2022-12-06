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
    public void StartGame_SetsRunningState()
    {
        var game = new CrazyEightsGame();
        game.AddPlayer(Some.PlayerName);
        game.AddPlayer(Some.OtherPlayerName);
        game.Start();
        Assert.That(game.State, Is.EqualTo(GameState.Running));
    }


    [Test]
    public void Command()
    {
        var game = RunningGame();

        game.PutCardOnPile(Guid.NewGuid(), Guid.NewGuid());
    }

    [Test]
    public void Command2()
    {
        var game = RunningGame();

        game.PutCardOnPile(game.CurrentPlayer.Id, Guid.NewGuid());
    }

    [Test]
    public void Command3()
    {
        var game = RunningGame();

        game.PutCardOnPile(game.CurrentPlayer.Id, Guid.NewGuid());
    }

    private static CrazyEightsGame RunningGame()
    {
        var game = new CrazyEightsGame();
        game.AddPlayer(Some.PlayerName);
        game.AddPlayer(Some.OtherPlayerName);
        game.Start();
        return game;
    }
    
}