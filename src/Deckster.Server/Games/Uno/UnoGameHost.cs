using System.Diagnostics.CodeAnalysis;
using Deckster.Server.Communication;
using Deckster.Server.Data;
using Deckster.Server.Games.Common;
using Deckster.Server.Games.Uno.Core;

namespace Deckster.Server.Games.Uno;

public class UnoGameHost : StandardGameHost<UnoGame>
{
    public override string GameType => "Uno";
    public override GameState State => Game.Value?.State ?? GameState.Waiting;

    public UnoGameHost(IRepo repo) : base(repo, new UnoProjection(), 4)
    {
    }

    protected override void ChannelDisconnected(IServerChannel channel)
    {
        
    }

    public override bool TryAddBot([MaybeNullWhen(true)] out string error)
    {
        error = "Bot not supported";
        return false;
    }
}