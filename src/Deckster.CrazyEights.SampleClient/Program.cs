namespace Deckster.CrazyEights.SampleClient;

class Program
{
    public static async Task<int> Main(string[] argz)
    {
        try
        {
            var host = argz[0];
            var accessToken = argz[1];
            using var cts = new CancellationTokenSource();
            var client = await CrazyEightsClient.ConnectAsync(host,accessToken, cts.Token);
            var ai = new CrazyEightsAi(client);
            await ai.PlayAsync(cts.Token);
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }
}