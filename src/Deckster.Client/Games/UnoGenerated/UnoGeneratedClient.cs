using System.Diagnostics;
using Deckster.Client.Communication;
using Deckster.Core.Protocol;
using Deckster.Core.Games.Common;
using Deckster.Core.Games.Uno;

namespace Deckster.Client.Games.Uno;

/**
 * Autogenerated by really, really eager small hamsters.
*/

[DebuggerDisplay("UnoClient {PlayerData}")]
public class UnoGeneratedClient(IClientChannel channel) : GameClient(channel)
{
    public event Action<GameStartedNotification>? GameStarted;
    public event Action<PlayerPutCardNotification>? PlayerPutCard;
    public event Action<PlayerPutWildNotification>? PlayerPutWild;
    public event Action<PlayerDrewCardNotification>? PlayerDrewCard;
    public event Action<PlayerPassedNotification>? PlayerPassed;
    public event Action<GameEndedNotification>? GameEnded;
    public event Action<ItsYourTurnNotification>? ItsYourTurn;

    public Task<PlayerViewOfGame> PutCard(PutCardRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<PlayerViewOfGame>(request, cancellationToken);
    }

    public Task<PlayerViewOfGame> PutWild(PutWildRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<PlayerViewOfGame>(request, cancellationToken);
    }

    public Task<UnoCardResponse> DrawCard(DrawCardRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<UnoCardResponse>(request, cancellationToken);
    }

    public Task<EmptyResponse> Pass(PassRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<EmptyResponse>(request, cancellationToken);
    }

    protected override void OnNotification(DecksterNotification notification)
    {
        try
        {
            switch (notification)
            {
                case GameStartedNotification m:
                    GameStarted?.Invoke(m);
                    return;
                case PlayerPutCardNotification m:
                    PlayerPutCard?.Invoke(m);
                    return;
                case PlayerPutWildNotification m:
                    PlayerPutWild?.Invoke(m);
                    return;
                case PlayerDrewCardNotification m:
                    PlayerDrewCard?.Invoke(m);
                    return;
                case PlayerPassedNotification m:
                    PlayerPassed?.Invoke(m);
                    return;
                case GameEndedNotification m:
                    GameEnded?.Invoke(m);
                    return;
                case ItsYourTurnNotification m:
                    ItsYourTurn?.Invoke(m);
                    return;
                default:
                    return;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

public static class UnoGeneratedClientDecksterClientExtensions
{
    public static GameApi<UnoGeneratedClient> Uno(this DecksterClient client)
    {
        return new GameApi<UnoGeneratedClient>(client.BaseUri.Append("uno"), client.Token, c => new UnoGeneratedClient(c));
    }
}