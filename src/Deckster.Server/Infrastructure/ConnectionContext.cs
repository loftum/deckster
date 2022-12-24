using System.Net.Sockets;
using Deckster.Communication;
using Deckster.Communication.Handshake;
using Deckster.Server.Users;

namespace Deckster.Server.Infrastructure;

public class ConnectionContext
{
    private readonly Socket _readSocket;
    private readonly Stream _readStream;
    private readonly Socket _writeSocket;
    private readonly Stream _writeStream;

    private DecksterCommunicator? _communicator;

    public User User { get; }
    public IServiceProvider Services { get; }
    public ConnectRequest Request { get; }
    public ConnectResponse Response { get; }
    
    public ConnectionContext(
        Socket readSocket,
        Stream readStream,
        Socket writeSocket,
        Stream writeStream,
        ConnectRequest request,
        User user,
        IServiceProvider services)
    {
        _readSocket = readSocket;
        _readStream = readStream;
        _writeSocket = writeSocket;
        _writeStream = writeStream;

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

    public DecksterCommunicator GetCommunicator()
    {
        _communicator ??= new DecksterCommunicator(_readStream, _writeStream, Response.PlayerData);
        
        return _communicator;
    }

    public void Close()
    {
        _readSocket.Dispose();
        _readStream.Dispose();
        _writeSocket.Dispose();
        _writeStream.Dispose();
        _communicator?.Dispose();
    }
}