using Deckster.Server.CodeGeneration.Code;

namespace Deckster.Generated.Client;

public abstract class ClientGenerator
{
    protected readonly SourceWriter SourceCode = new();

    public async Task WriteToAsync(string path)
    {
        var file = new FileInfo(path);
        if (file.Directory is { Exists: false })
        {
            file.Directory.Create();
        }
        await using var fileStream = file.Exists ? file.Open(FileMode.Truncate) : file.Open(FileMode.CreateNew);
        await using var writer = new StreamWriter(fileStream);
        await writer.WriteAsync(SourceCode.ToString());
        await writer.FlushAsync();
    }
}