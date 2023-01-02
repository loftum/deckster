using Deckster.Communication;

namespace Deckster.CrazyEights;

public static class CrazyEightsClientFactory
{
    public static async Task<CrazyEightsClient> ConnectAsync(Uri uri, CancellationToken cancellationToken)
    {
        var communicator = await DecksterClient.ConnectAsync(uri, cancellationToken);
        var client = new CrazyEightsClient(communicator);
        if (uri.AbsolutePath.EndsWith("practice"))
        {
            await communicator.SendAsync(new StartCommand(), cancellationToken);
        }
        return client;
    }
}