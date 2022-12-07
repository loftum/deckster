using Deckster.Core.Domain;

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

            var services = builder.Services;
            ConfigureServices(services);

            ConfigurePipeline(builder);
            
            var app = builder.Build();
            ConfigurePipeline(app);

            await app.RunAsync(cts.Token);
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        app.MapControllers();
        app.UseAuthentication();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSingleton<UserRepo>();
        services.AddCrazyEights();
    }
}