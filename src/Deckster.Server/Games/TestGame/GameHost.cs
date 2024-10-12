using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Deckster.Client.Common;
using Deckster.Client.Protocol;
using Deckster.Client.Serialization;
using Deckster.Server.Communication;
using Deckster.Server.Games.Common;

namespace Deckster.Server.Games.TestGame;

public abstract class GameHost<TRequest, TResponse, TNotification> : IGameHost
    where TRequest : IHaveDiscriminator
    where TResponse : IHaveDiscriminator
    where TNotification : IHaveDiscriminator

{
    public event EventHandler<IGameHost>? OnEnded;
    public abstract string GameType { get; }
    public string Name { get; init; }
    public abstract GameState State { get; }

    protected readonly JsonSerializerOptions JsonOptions = DecksterJson.Create(o =>
    {
        o.Converters.Add(new DerivedTypeConverter<TRequest>());
        o.Converters.Add(new DerivedTypeConverter<TResponse>());
        o.Converters.Add(new DerivedTypeConverter<TNotification>());
    });

    public abstract Task Start();

    public abstract bool TryAddPlayer(IServerChannel channel, [MaybeNullWhen(true)] out string error);
    public abstract Task CancelAsync();
    public abstract ICollection<PlayerData> GetPlayers();
}