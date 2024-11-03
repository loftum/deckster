using Deckster.Client.Protocol;
using Deckster.Server.CodeGeneration;
using Deckster.Server.CodeGeneration.Meta;
using Deckster.Server.Games;

namespace Deckster.Generated.Client;

public class Program
{
    public static async Task<int> Main(string[] argz)
    {
        try
        {
            Console.WriteLine("Generating code!");
            var projectPath = GetProjectPath(argz);
            var openapi = new OpenApiDocumentGenerator(typeof(DecksterMessage));
            await openapi.WriteAsJsonAsync(Path.Combine(projectPath, "deckster.openapi.json"));
            await openapi.WriteAsYamlAsync(Path.Combine(projectPath, "deckster.openapi.yaml"));

            var baseType = typeof(GameObject);
            var types = baseType.Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))
                .ToArray();

            await GenerateClientsAsync(projectPath, types);
            
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    private static string GetProjectPath(string[] argz)
    {
        if (argz.Any())
        {
            return argz[0];
        }

        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory != null && directory.GetFiles("Deckster.Generated.Client.csproj").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find project path for Deckster.Generated.Client.csproj");
    }

    private static async Task GenerateClientsAsync(string projectPath, Type[] types)
    {
        foreach (var type in types)
        {
            if (GameMeta.TryGetFor(type, out var game))
            {
                var path = Path.Combine(projectPath, "..", "src", "Deckster.Client", "Games", game.Name);
                await GenerateCsharpAsync(path, type, game);
                await GenerateKotlinAsync(Path.Combine(projectPath, "kotlin"), type, game);
            }
        }
    }

    private static async Task GenerateCsharpAsync(string basePath, Type type, GameMeta game)
    {
        var ns = type.Namespace?.Split('.').LastOrDefault() ?? throw new Exception($"OMG CANT HAZ NAEMSPAZE OF ITZ TAYP '{type.Name}'");
        var path = Path.Combine(basePath, "Generated", $"{game.Name}GeneratedClient.cs");
                
        Console.WriteLine(path);
        var kotlin = new CsharpClientGenerator(game, $"Deckster.Client.Games.{ns}");
        await kotlin.WriteToAsync(path);
    }

    private static async Task GenerateKotlinAsync(string basePath, Type type, GameMeta game)
    {
        if (Directory.Exists(basePath))
        {
            Directory.Delete(basePath, true);
        }

        Directory.CreateDirectory(basePath);
        
        var ns = type.Namespace?.Split('.').LastOrDefault()?.ToLowerInvariant() ?? throw new Exception($"OMG CANT HAZ NAEMSPAZE OF ITZ TAYP '{type.Name}'");
        var path = Path.Combine(basePath, "no.forse.decksterlib",  ns, $"{game.Name}Client.kt");
                
        Console.WriteLine(path);
        var kotlin = new KotlinClientGenerator(game, $"no.forse.decksterlib.{ns}");
        await kotlin.WriteToAsync(path);
    }
}
