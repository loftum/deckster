using System.Text.Json;
using System.Text.Json.Serialization;
using Deckster.Communication;
using Deckster.Core.Domain;
using Deckster.Core.Games;

namespace Deckster.CrazyEights;

public class CrazyEightsClient
{
    public event Func<PlayerPutCardMessage, Task>? PlayerPutCard;
    public event Func<PlayerPutEightMessage, Task>? PlayerPutEight;
    public event Func<PlayerDrewCardMessage, Task>? PlayerDrewCard;
    public event Func<PlayerPassedMessage, Task>? PlayerPassed;
    public event Func<ItsYourTurnMessage, Task>? ItsYourTurn;
    
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = {new JsonStringEnumConverter()},
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    private readonly DecksterCommunicator _communicator;

    private CrazyEightsClient(DecksterCommunicator communicator)
    {
        _communicator = communicator;
        communicator.OnMessage += HandleMessageAsync;
    }

    public async Task<CommandResult?> PutCardAsync(Card card, CancellationToken cancellationToken = default)
    {
        var message = new PlayerPutCardMessage
        {
            PlayerId = _communicator.PlayerData.PlayerId,
            Card = card
        };

        return await _communicator.SendJsonAsync<PlayerPutCardMessage, CommandResult>(message, Options, cancellationToken);
    }

    private Task HandleMessageAsync(byte[] bytes)
    {
        try
        {
            var message = JsonSerializer.Deserialize<CrazyEightsMessage>(bytes, Options);
            return message switch
            {
                PlayerPutCardMessage m when PlayerPutCard != null => PlayerPutCard(m),
                PlayerPutEightMessage m when PlayerPutEight != null => PlayerPutEight(m),
                PlayerDrewCardMessage m when PlayerDrewCard != null => PlayerDrewCard(m),
                PlayerPassedMessage m when PlayerPassed != null => PlayerPassed(m),
                ItsYourTurnMessage m when ItsYourTurn != null => ItsYourTurn(m),
                _ => Task.CompletedTask
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Task.CompletedTask;
        }
    }

    public static async Task<CrazyEightsClient> ConnectAsync(string host, string accessToken, CancellationToken cancellationToken)
    {
        var communicator = await DecksterClient.ConnectAsync(host, DecksterConstants.TcpPort, accessToken, cancellationToken);
        return new CrazyEightsClient(communicator);
    }
}