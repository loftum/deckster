using Deckster.Core.Serialization;
using Deckster.Games.Uno;
using Deckster.Server.Data;
using Deckster.Server.Games.Idiot;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Deckster.UnitTests.Games.Idiot;

public class IdiotGameHostTest
{
    [Test]
    public async ValueTask RunGame()
    {
        var repo = new InMemoryRepo();
        var host = new IdiotGameHost(repo, new NullLoggerFactory());

        for (var ii = 0; ii < 4; ii++)
        {
            if (!host.TryAddBot(out var error))
            {
                Assert.Fail(error);
            }    
        }
        
        try
        {
            Console.WriteLine("Starting");
            await host.RunAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            var thing = repo.EventThings.Values.Cast<InMemoryEventQueue<UnoGame>>().SingleOrDefault();
            if (thing != null)
            {
                foreach (var evt in thing.Events)
                {
                    Console.WriteLine(evt.Pretty());
                }
            }
        }
    }
}