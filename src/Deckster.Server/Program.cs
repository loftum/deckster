namespace Deckster.Server;

class Program
{
    public static async Task<int> Main(string[] argz)
    {
        try
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (o, e) => cts.Cancel();
            var builder = WebApplication.CreateBuilder(argz);
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            await app.RunAsync();
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }
}