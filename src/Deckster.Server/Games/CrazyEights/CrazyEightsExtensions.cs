namespace Deckster.Server.Games.CrazyEights;

public static class CrazyEightsExtensions
{
    public static IServiceCollection AddCrazyEights(this IServiceCollection services)
    {
        services.AddSingleton<CrazyEightsGameRegistry>();
        return services;
    }
}