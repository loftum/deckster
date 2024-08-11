using Deckster.Server.Authentication;
using Deckster.Server.Games.CrazyEights;
using Deckster.Server.Users;
using Microsoft.AspNetCore.WebSockets;

namespace Deckster.Server;

class Program
{
    public static async Task<int> Main(string[] argz)
    {
        try
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, _) => cts.Cancel();
            var builder = WebApplication.CreateBuilder(argz);

            var services = builder.Services;
            ConfigureServices(services);

            await using var web = builder.Build();
            Configure(web);

            await web.RunAsync(cts.Token);
            
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unhandled: {e}");
            return 1;
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddWebSockets(o =>
        {
            o.KeepAliveInterval = TimeSpan.FromMinutes(1);
        });
        services.AddSignalR();
        services.AddControllers();
        services.AddSingleton<UserRepo>();
        services.AddCrazyEights();

        var mvc = services.AddMvc();
        mvc.AddRazorRuntimeCompilation();
        
        services.AddAuthentication()
            .AddCookie(AuthenticationSchemes.Cookie, o =>
            {
                o.LoginPath = "/login";
                o.LogoutPath = "/logout";
            });

    }
    
    private static void Configure(WebApplication app)
    {
        app.UseStaticFiles();
        app.MapControllers();
        app.UseAuthentication();
    }
}

