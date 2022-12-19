using Deckster.Communication;
using Deckster.Server.Users;

namespace Deckster.Server.Infrastructure;

public class DecksterContext
{
    public User User { get; init; }
    public DecksterCommunicator Communicator { get; init; }
    public string Path { get; set; }
    public IServiceProvider Services { get; init; }
}