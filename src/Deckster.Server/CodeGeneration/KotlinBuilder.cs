using Deckster.Server.CodeGeneration.Code;
using Deckster.Server.CodeGeneration.Meta;
using StringExtensions = Deckster.Client.Sugar.StringExtensions;

namespace Deckster.Server.CodeGeneration;

public class KotlinGenerator
{
    private readonly SourceWriter _sourceCode = new();

    public KotlinGenerator(GameClientMeta meta, string ns)
    {
        _sourceCode.AppendLine($"package {ns}")
            .AppendLine()
            .AppendLine($"interface {meta.Name} {{");

        using (_sourceCode.Indent())
        {
            foreach (var method in meta.Methods)
            {
                _sourceCode.Append($"suspend fun {StringExtensions.ToCamelCase(method.Name)}({string.Join(", ", method.Parameters.Select(FormatParameter))})");
                if (method.ReturnType.Name != "void")
                {
                    _sourceCode.Append($": {method.ReturnType}");
                }

                _sourceCode.AppendLine();
            }
        }
        
        _sourceCode.AppendLine("}");
    }

    public async Task WriteToAsync(string path)
    {
        var file = new FileInfo(path);
        if (file.Directory is { Exists: false })
        {
            file.Directory.Create();
        }
        await using var fileStream = file.Exists ? file.Open(FileMode.Truncate) : file.Open(FileMode.CreateNew);
        await using var writer = new StreamWriter(fileStream);
        await writer.WriteAsync(_sourceCode.ToString());
        await writer.FlushAsync();
    }
    
    private static string FormatParameter(ParameterMeta parameter)
    {
        return $"{parameter.Name}: {parameter.Type}";
    }
}