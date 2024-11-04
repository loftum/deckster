using Deckster.CodeGenerator.CSharp;
using Deckster.Core;

namespace Deckster.CodeGenerator.Generators;

public class CsharpClientGenerator : ClientGenerator
{
    public string ClientName { get; }
    
    public CsharpClientGenerator(CSharpGameMeta meta, string ns)
    {
        ClientName = $"{meta.Name}GeneratedClient";

        var usings = new HashSet<string>(meta.Usings.Concat([
            "System.Diagnostics",
            "Deckster.Client.Communication",
            "Deckster.Core.Protocol",
            "Deckster.Core.Games.Common",
            $"Deckster.Core.Games.{meta.Name}"
        ]));

        foreach (var u in usings)
        {
            SourceCode.AppendLine($"using {u};");
        }
        
        SourceCode
            .AppendLine()
            .AppendLine($"namespace {ns};")
            .AppendLine()
            .AppendLine("/**")
            .AppendLine(" * Autogenerated by really, really eager small hamsters.")
            .AppendLine("*/")
            .AppendLine()
            .AppendLine($"[DebuggerDisplay(\"{meta.Name}Client {{PlayerData}}\")]")
            .AppendLine($"public class {ClientName}(IClientChannel channel) : GameClient(channel)");

        using (SourceCode.StartBlock())
        {
            foreach (var notification in meta.Notifications)
            {
                SourceCode.AppendLine($"public event Action<{notification.Message.Name}>? {notification.Name};");
            }
            
            SourceCode.AppendLine();

            foreach (var method in meta.Methods)
            {
                var parameters = new []{$"{method.Parameter.ParameterType} {method.Parameter.Name}", "CancellationToken cancellationToken = default"}
                    .StringJoined(", ");
                SourceCode.AppendLine($"public Task<{method.ReturnType.Name}> {method.Name}({parameters})");
                using (SourceCode.StartBlock())
                {
                    var p = new[] {$"{method.Parameter.Name}", "cancellationToken"};
                    SourceCode.AppendLine($"return SendAsync<{method.ReturnType.Name}>({p});");
                }

                SourceCode.AppendLine();
            }

            SourceCode.AppendLine("protected override void OnNotification(DecksterNotification notification)");
            using (SourceCode.StartBlock())
            {
                SourceCode.AppendLine("try");
                using (SourceCode.StartBlock())
                {
                    SourceCode.AppendLine("switch (notification)");
                    using (SourceCode.StartBlock())
                    {
                        foreach (var notification in meta.Notifications)
                        {
                            SourceCode.AppendLine($"case {notification.Message.Name} m:");
                            using (SourceCode.Indent())
                            {
                                SourceCode
                                    .AppendLine($"{notification.Name}?.Invoke(m);")
                                    .AppendLine("return;");
                            }
                        }

                        SourceCode.AppendLine("default:");
                        using (SourceCode.Indent())
                        {
                            SourceCode.AppendLine("return;");
                        }
                    }
                }

                SourceCode.AppendLine("catch (Exception e)");
                using (SourceCode.StartBlock())
                {
                    SourceCode.AppendLine("Console.WriteLine(e);");
                }
            }
        }

        SourceCode.AppendLine();

        SourceCode.AppendLine($"public static class {ClientName}Extensions");
        using (SourceCode.StartBlock())
        {
            
        }
        

        SourceCode.AppendLine();

        SourceCode.AppendLine($"public static class {ClientName}DecksterClientExtensions");
        using (SourceCode.StartBlock())
        {
            SourceCode.AppendLine($"public static GameApi<{ClientName}> {meta.Name}(this DecksterClient client)");
            using (SourceCode.StartBlock())
            {
                SourceCode.AppendLine($"return new GameApi<{ClientName}>(client.BaseUri.Append(\"{meta.Name.ToLowerInvariant()}\"), client.Token, c => new {ClientName}(c));");
            }
        }

    }
}