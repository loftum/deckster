using Deckster.Server.CodeGeneration.Meta;

namespace Deckster.Generated.Client;

public class CsharpGenerator : ClientGenerator
{
    public CsharpGenerator(GameMeta meta, string ns)
    {
        SourceCode
            .AppendLine("/**")
            .AppendLine(" * Autogenerated by really, really eager small hamsters.")
            .AppendLine("*/")
            .AppendLine($"namespace {ns};")
            .AppendLine()
            .AppendLine($"[DebuggerDisplay(\"{meta.Name}Client {{PlayerData}}\")]")
            .AppendLine($"public class {meta.Name}Client : GameClient");

        using (SourceCode.StartBlock())
        {
            foreach (var notification in meta.Notifications)
            {
                SourceCode.AppendLine($"public event Action<{notification.Name}>? {notification.Name};");
            }
        }

        SourceCode.AppendLine();

        SourceCode.AppendLine($"public {meta.Name}(IClientChannel channel) : base(channel){{}}");

        foreach (var method in meta.Methods)
        {
            var parameters = method.Parameters
                .Select(p => $"{p.Type.Name} {p.Name}")
                .Append("CancellationToken cancellationToken = default")
                .StringJoined(", ");
            SourceCode.AppendLine($"public Task<{method.ReturnType.Name}> {method.Name}({parameters})");
            using (SourceCode.StartBlock())
            {
                var p = method.Parameters.Select(p => p.Name).Append("cancellationToken").StringJoined(", ");
                SourceCode.AppendLine($"return SendAsync<{method.ReturnType.Name}>({p})");
            }
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
                            SourceCode.AppendLine($"{notification.Name}?.Invoke(m);");
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
}