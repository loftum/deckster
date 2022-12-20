using Deckster.Communication;
using NUnit.Framework;

namespace Deckster.UnitTests;

[TestFixture]
public class StreamExtensionsTest
{
    [Test]
    [TestCase(0)]
    [TestCase(int.MaxValue)]
    [TestCase(int.MinValue)]
    public void ToInt(int value)
    {
        var bytes = value.ToBytes();
        var intValue = bytes.ToInt();
        
        Assert.That(intValue, Is.EqualTo(value));
    }
}