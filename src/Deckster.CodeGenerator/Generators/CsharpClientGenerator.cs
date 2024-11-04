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
            Source.AppendLine($"using {u};");
        }
        
        Source
            .AppendLine()
            .AppendLine($"namespace {ns};")
            .AppendLine()
            .AppendLine("/**")
            .AppendLine(" * Autogenerated by really, really eager small hamsters.")
            .AppendLine("*/")
            .AppendLine()
            .AppendLine($"[DebuggerDisplay(\"{meta.Name}Client {{PlayerData}}\")]")
            .AppendLine($"public class {ClientName}(IClientChannel channel) : GameClient(channel)");

        using (Source.CodeBlock())
        {
            foreach (var notification in meta.Notifications)
            {
                Source.AppendLine($"public event Action<{notification.MessageType.Name}>? {notification.Name};");
            }
            
            Source.AppendLine();

            foreach (var method in meta.Methods)
            {
                var parameters = new []{$"{method.Request.ParameterType.Name} {method.Request.Name}", "CancellationToken cancellationToken = default"}
                    .StringJoined(", ");
                Source.AppendLine($"public Task<{method.ResponseType.Name}> {method.Name}({parameters})");
                using (Source.CodeBlock())
                {
                    Source.AppendLine($"return SendAsync<{method.ResponseType.Name}>({method.Request.Name}, false, cancellationToken);");
                }

                Source.AppendLine();
            }

            Source.AppendLine("protected override void OnNotification(DecksterNotification notification)");
            using (Source.CodeBlock())
            {
                Source.AppendLine("try");
                using (Source.CodeBlock())
                {
                    Source.AppendLine("switch (notification)");
                    using (Source.CodeBlock())
                    {
                        foreach (var notification in meta.Notifications)
                        {
                            Source.AppendLine($"case {notification.MessageType.Name} m:");
                            using (Source.Indent())
                            {
                                Source
                                    .AppendLine($"{notification.Name}?.Invoke(m);")
                                    .AppendLine("return;");
                            }
                        }

                        Source.AppendLine("default:");
                        using (Source.Indent())
                        {
                            Source.AppendLine("return;");
                        }
                    }
                }

                Source.AppendLine("catch (Exception e)");
                using (Source.CodeBlock())
                {
                    Source.AppendLine("Console.WriteLine(e);");
                }
            }
        }

        Source.AppendLine();

        Source.AppendLine($"public static class {ClientName}Conveniences");
        using (Source.CodeBlock())
        {
            foreach (var extension in meta.ExtensionMethods)
            {
                var parameters = extension.Parameters.Select(p => $"{p.ParameterType.Name} {p.Name}")
                    .Append("CancellationToken cancellationToken = default")
                    .StringJoined(", ");


                if (extension.ReturnParameters == null)
                {
                    Source.AppendLine($"public static Task<{extension.ReturnType.Name}> {extension.Name}(this {ClientName} self, {parameters})");
                    using(Source.CodeBlock())
                    {
                        var properties = extension.Parameters.Select(p => $"{p.Name.ToPascalCase()} = {p.Name}").StringJoined(", ");
                        Source.AppendLine($"var request = new {extension.Method.Request.ParameterType.Name}{{ {properties} }};");
                        Source.AppendLine($"return self.SendAsync<{extension.ReturnType.Name}>(request, true, cancellationToken);");
                    }    
                }
                else
                {
                    switch (extension.ReturnParameters.Length)
                    {
                        case 0:
                            Source.AppendLine($"public static async Task {extension.Name}(this {ClientName} self, {parameters})");
                            break;
                        case 1:
                            Source.AppendLine($"public static async Task<{extension.ReturnParameters[0].ParameterType.Name}> {extension.Name}(this {ClientName} self, {parameters})");
                            break;
                        default:
                            var returnTuple = extension.ReturnParameters.Select(p => $"{p.ParameterType.ToDisplayString()} {p.Name}").StringJoined(", ");
                            Source.AppendLine($"public static async Task<({returnTuple})> {extension.Name}(this {ClientName} self, {parameters})");
                            break;
                    }

                    using (Source.CodeBlock())
                    {
                        var properties = extension.Parameters.Select(p => $"{p.Name.ToPascalCase()} = {p.Name}").StringJoined(", ");
                        Source.AppendLine($"var request = new {extension.Method.Request.ParameterType.Name}{{ {properties} }};");
                        Source.AppendLine($"var response = await self.SendAsync<{extension.Method.ResponseType.Name}>(request, true, cancellationToken);");

                        switch (extension.ReturnParameters.Length)
                        {
                            case 0:
                                break;
                            case 1:
                                Source.AppendLine($"return response.{extension.ReturnParameters[0].Name.ToPascalCase()};");
                                break;
                            default:
                                var returnTuple = extension.ReturnParameters.Select(p => $"response.{p.Name.ToPascalCase()}").StringJoined(", ");
                                Source.AppendLine($"return ({returnTuple});");
                                break;
                        }
                    }
                }
            }
        }

        Source.AppendLine();

        Source.AppendLine($"public static class {ClientName}DecksterClientExtensions");
        using (Source.CodeBlock())
        {
            Source.AppendLine($"public static GameApi<{ClientName}> {meta.Name}(this DecksterClient client)");
            using (Source.CodeBlock())
            {
                Source.AppendLine($"return new GameApi<{ClientName}>(client.BaseUri.Append(\"{meta.Name.ToLowerInvariant()}\"), client.Token, c => new {ClientName}(c));");
            }
        }
    }
}