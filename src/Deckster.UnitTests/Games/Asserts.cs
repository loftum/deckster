using Deckster.Client.Games.Common;
using Deckster.Client.Protocol;
using NUnit.Framework;

namespace Deckster.UnitTests.Games;

public static class Asserts
{
    public static void Success(DecksterResponse result)
    {
        switch (result)
        {
            case FailureResponse r:
                Assert.Fail($"Expeced success, but got '{r.Message}'");
                break;
        }
    }

    public static void Fail(DecksterResponse response, string message)
    {
        switch (response)
        {
            case { HasError: true }:
            {
                Assert.That(response.Error, Is.EqualTo(message));
                break;
            }
            case FailureResponse r:
            {
                Assert.That(r.Message, Is.EqualTo(message));
                break;
            }
            default:
                Assert.Fail($"Expected failure, but got {response.GetType().Name}");
                break;
        }
    }
}