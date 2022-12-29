using Deckster.Communication;
using Deckster.Communication.Handshake;
using Deckster.Server.Users;

namespace Deckster.Server.Infrastructure;

public class ConnectionContext
{
    public IDecksterCommunicator Communicator { get; }
    public User User { get; }
    public IServiceProvider Services { get; }
    public ConnectRequest Request { get; }
    public ConnectResponse Response { get; }
    
    public ConnectionContext(
        IDecksterCommunicator communicator,
        ConnectRequest request,
        User user,
        IServiceProvider services)
    {
        Communicator = communicator;
        Request = request;
        User = user;
        Services = services;
        
        Response = new ConnectResponse
        {
            StatusCode = 200,
            Description = "OK",
            PlayerData = new PlayerData
            {
                Name = user.Name,
                PlayerId = user.Id
            }
        };
    }

    public void Close()
    {
        Communicator.Dispose();
    }
}