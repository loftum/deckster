using System.Diagnostics.CodeAnalysis;
using Deckster.Client.Games.ChatRoom;
using Deckster.Client.Serialization;
using Deckster.Server.Communication;
using Deckster.Server.Data;
using Deckster.Server.Games.Common;
using Deckster.Server.Games.CrazyEights.Core;

namespace Deckster.Server.Games.ChatRoom;

public class ChatRoomHost : GameHost<ChatRequest, ChatResponse, ChatNotification>
{
    public override string GameType => "ChatRoom";
    public override GameState State => GameState.Running;

    private readonly IRepo _repo;
    private readonly IEventStream _events;
    private readonly Chat _chat;

    public ChatRoomHost(IRepo repo)
    {
        _repo = repo;
        var started = new ChatCreated();
        _events = repo.StartEventStream<Chat>(started.Id, started);
        _chat = Chat.Create(started);
        _events.Append(started);
    }

    public override Task StartAsync()
    {
        return Task.CompletedTask;
    }

    private async void MessageReceived(IServerChannel channel, ChatRequest request)
    {
        var player = channel.Player;
        Console.WriteLine($"Received: {request.Pretty()}");


        var context = new TurnContext();
        switch (request)
        {
            case SendChatMessage message:
                await _chat.HandleAsync(message, context);
                _events.Append(message);
                await _events.SaveChangesAsync();
                // await _repo.SaveAsync(_chat);
                await _players[player.Id].ReplyAsync(context.Response, JsonOptions);
                foreach (var notification in context.Notifications)
                {
                    await BroadcastMessageAsync(notification);
                }
                return;
        }
    }

    public override bool TryAddPlayer(IServerChannel channel, [MaybeNullWhen(true)] out string error)
    {
        if (!_players.TryAdd(channel.Player.Id, channel))
        {
            Console.WriteLine($"Could not add player {channel.Player.Name}");
            error = "Player already exists";
            return false;
        }
        
        Console.WriteLine($"Added player {channel.Player.Name}");
        channel.Disconnected += ChannelDisconnected;
        
        channel.Start<ChatRequest>(MessageReceived, JsonOptions, Cts.Token);

        error = default;
        return true;
    }

    private async void ChannelDisconnected(IServerChannel channel)
    {
        Console.WriteLine($"{channel.Player.Name} disconnected");
        _players.Remove(channel.Player.Id, out _);
        await BroadcastMessageAsync(new ChatNotification
        {
            Sender = channel.Player.Name,
            Message = "Disconnected"
        });
    }
}