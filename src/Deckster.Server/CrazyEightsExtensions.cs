using Deckster.Core.Games.CrazyEights;

namespace Deckster.Server;

public static class CrazyEightsExtensions
{
    public static IServiceCollection AddCrazyEights(this IServiceCollection services)
    {
        services.AddSingleton<CrazyEightsRepo>();
    }
}