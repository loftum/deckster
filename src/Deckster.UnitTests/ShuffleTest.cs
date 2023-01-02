using Deckster.Server.Games.CrazyEights;
using NUnit.Framework;

namespace Deckster.UnitTests;

[TestFixture]
public class ShuffleTest
{
    [Test]
    public void KnuthShuffleTest()
    {
        var unshuffled = Deck.Default.Cards;
        var shuffled = Deck.Default.Cards.KnuthShuffle();

        Assert.That(unshuffled.SequenceEqual(shuffled), Is.False);
    }
}