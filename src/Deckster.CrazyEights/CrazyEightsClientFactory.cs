using Deckster.Communication;

namespace Deckster.CrazyEights;

public static class CrazyEightsClientFactory
{
    public static async Task<CrazyEightsClient> ConnectAsync(Uri uri, CancellationToken cancellationToken)
    {
        var communicator = await DecksterClient.ConnectAsync(uri, cancellationToken);
        return new CrazyEightsClient(communicator);
    }
}