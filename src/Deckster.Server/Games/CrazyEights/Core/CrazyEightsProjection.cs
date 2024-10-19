using Deckster.Client.Games.CrazyEights;
using Marten;
using Marten.Events.Aggregation;
using Marten.Events.Projections;

namespace Deckster.Server.Games.CrazyEights.Core;

public class CrazyEightsProjection : SingleStreamProjection<CrazyEightsGame>
{
    public CrazyEightsGame Create(IList<CrazyEightsPlayer> players)
    {
        var game = new CrazyEightsGame
        {
            Players = players.ToList(),
            Deck = Deck.Standard
        };
        
        game.Reset();
        return game;
    }
    
    public void Apply(PutCardRequest request, CrazyEightsGame game)
    {
        game.PutCard()
    }
}


public class Hest
{
    public Guid Id { get; init; }
    public int KneggCount { get; set; }
    public int VrinskCount { get; set; }
}

public class HestProjection : SingleStreamProjection<Hest>
{
    public void Apply(KneggEvent e, Hest hest)
    {
        hest.KneggCount++;
    }

    public void Apply(VrinskEvent e, Hest hest)
    {
        hest.VrinskCount++;
    }
}

public class KneggEvent;
public class VrinskEvent;

public class HestStory
{
    public static async Task Tell()
    {
        var store = DocumentStore.For(o =>
        {
            o.Projections.Add<HestProjection>(ProjectionLifecycle.Inline);
        });
        await using var session = store.LightweightSession();

        var knegg = new KneggEvent();
        var vrinsk = new VrinskEvent();
        var stream = session.Events.StartStream<HestProjection>();
        

        for (var ii = 0; ii < 100; ii++)
        {
            session.Events.Append(stream.Id, knegg);
            session.Events.Append(stream.Id, vrinsk);
        }
        
        await session.SaveChangesAsync();
    }
}