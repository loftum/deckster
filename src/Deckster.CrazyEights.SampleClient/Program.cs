using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Deckster.Client.Common;
using Deckster.Client.Communication;
using Deckster.Client.Games.CrazyEights;
using Deckster.Client.Serialization;

namespace Deckster.CrazyEights.SampleClient;

class Program
{
    public static async Task<int> Main(string[] argz)
    {
        if (!TryGetUrl(argz, out var uri))
        {
            PrintUsage();
            return 0;
        }
        
        try
        {
            using var cts = new CancellationTokenSource();
            using var channel = await WebSocketDecksterChannel.ConnectAsync(uri, new PlayerData
            {
                
            },
                "",
            cts.Token);

            var message = new TestMessage
            {
                Word = "hest"
            };

            Console.WriteLine("SendAsync");
            await channel.SendAsync(message, cts.Token);
            var response = await channel.ReceiveAsync<TestMessage>(cts.Token);
            Console.WriteLine("ReceiveAsync");
            
            
            Console.WriteLine(Jsons.Pretty(response));
            await channel.DisconnectAsync(cts.Token);
            
            // var ai = new CrazyEightsPoorAi(new CrazyEightsClient(channel));
            // await ai.PlayAsync(cts.Token);
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 1;
        }
    }

    private static bool TryGetUrl(string[] args, [MaybeNullWhen(false)] out Uri uri)
    {
        foreach (var a in args)
        {
            if (Uri.TryCreate(a, UriKind.Absolute, out uri))
            {
                return true;
            }
        }

        uri = new Uri("ws://localhost:13992/wstest");
        return true;
    }

    private static void PrintUsage()
    {
        var usage = new StringBuilder()
            .AppendLine("Usage:")
            .AppendLine($"{Process.GetCurrentProcess().ProcessName} <uri>")
            .AppendLine($"e.g {Process.GetCurrentProcess().ProcessName} deckster://localhost:23023/crazyeights/123456");
        Console.WriteLine(usage);
    }
}

public class TestMessage
{
    public string Word { get; set; }
}