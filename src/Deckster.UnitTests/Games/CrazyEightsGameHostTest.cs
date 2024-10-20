using System.Diagnostics;
using Deckster.Server.Data;
using Deckster.Server.Games.Common;
using Deckster.Server.Games.CrazyEights;
using NUnit.Framework;

namespace Deckster.UnitTests.Games;

public class CrazyEightsGameHostTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Trace.Flush();
    }
    
    [Test]
    public async ValueTask Game()
    {
        var host = new CrazyEightsGameHost(new InMemoryRepo());

        Console.WriteLine("Add bot");
        if (!host.TryAddBot(out var error))
        {
            Assert.Fail(error);
        }
        Console.WriteLine("Add bot");
        if (!host.TryAddBot(out error))
        {
            Assert.Fail(error);
        }
        Console.WriteLine("Starting");
        await host.StartAsync();

        Console.WriteLine("Running");
        while (host.State != GameState.Finished)
        {
            await Task.Delay(1000);
        }
    }
}