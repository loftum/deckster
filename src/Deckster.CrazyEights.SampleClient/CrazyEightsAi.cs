namespace Deckster.CrazyEights.SampleClient;

public class CrazyEightsAi
{
    private readonly CrazyEightsClient _client;

    public CrazyEightsAi(CrazyEightsClient client)
    {
        _client = client;
    }

    public async Task PlayAsync(CancellationToken cancellationToken = default)
    {
        
    }
}