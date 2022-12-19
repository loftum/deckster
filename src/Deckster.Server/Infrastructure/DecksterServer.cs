using System.Net;
using System.Net.Sockets;
using Deckster.Communication;
using Deckster.Communication.Handshake;
using Deckster.Server.Users;

namespace Deckster.Server.Infrastructure;

public class DecksterServer : IDisposable
{
    private readonly int _port;
    private readonly ILogger _logger = Log.Factory.CreateLogger<DecksterServer>();
    private readonly IServiceProvider _services;
    private readonly UserRepo _userRepo;
    private readonly TcpListener _listener;
    private readonly List<DecksterCommunicator> _communicators = new();

    private readonly DecksterDelegate _pipeline;

    public DecksterServer(int port, IServiceProvider services, DecksterDelegate pipeline)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Any, port);
        _userRepo = services.GetRequiredService<UserRepo>();
        _pipeline = pipeline;
        _services = services;
    }
    
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _listener.Start();
            _logger.LogInformation("Listening for connections on port {port}", _port);
            while (!cancellationToken.IsCancellationRequested)
            {
                var socket = await _listener.AcceptSocketAsync(cancellationToken);
                ConnectAsync(socket, cancellationToken);
            }
        }
        finally
        {
            _logger.LogInformation("Stop listening for connections on port {port}", _port);
            _listener.Stop();
        }
    }

    private async void ConnectAsync(Socket socket, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            var communicator = await HandshakeAsync(socket, cts.Token);
            if (communicator != null)
            {
                try
                {
                    _communicators.Add(communicator);
                    communicator.OnDisconnected += OnDisconnected;
                    var context = new DecksterContext
                    {
                        Services = _services,
                        Communicator = communicator
                    };
                    await _pipeline(context);
                }
                catch (Exception e)
                {
                    communicator.OnDisconnected -= OnDisconnected;
                    Remove(communicator);
                    _logger.LogError(e, "Unhandled exception in connection pipeline");
                }
            }
        }
        catch (Exception e)
        {
            socket.Dispose();
            _logger.LogError(e, "Unhandled handshake exception");
        }
    }

    private Task OnDisconnected(DecksterCommunicator communicator)
    {
        communicator.OnDisconnected -= OnDisconnected;
        Remove(communicator);
        return Task.CompletedTask;
    }

    private void Remove(DecksterCommunicator communicator)
    {
        _communicators.Remove(communicator);
        communicator.Dispose();
    }

    private async Task<DecksterCommunicator?> HandshakeAsync(Socket socket, CancellationToken cancellationToken = default)
    {
        if (socket.RemoteEndPoint is not IPEndPoint endpoint)
        {
            _logger.LogError("Expected client endpoint to be IPEndPoint, but was {actualType} {actual}", socket.RemoteEndPoint?.GetType().Name, socket.RemoteEndPoint);
            socket.Dispose();
            return null;
        }
        _logger.LogInformation("Got connection from {endpoint}", endpoint);
        
        // 1. Read client hello
        var stream = new NetworkStream(socket);
        var clientHello = await stream.ReceiveJsonAsync<ClientHelloMessage>(DecksterJson.Options, cancellationToken);
        if (clientHello == null)
        {
            _logger.LogError("Invalid client hello");
            await stream.SendJsonAsync(new ServerHelloFailure {ErrorMessage = "Invalid client hello message"}, DecksterJson.Options, cancellationToken);
            socket.Dispose();
            return null;
        }

        var user = await _userRepo.GetByTokenAsync(clientHello.AccessToken, cancellationToken);
        if (user == null)
        {
            _logger.LogError("Invalid access token '{token}'. No user found", clientHello.AccessToken);
            await stream.SendJsonAsync(new ServerHelloFailure {ErrorMessage = "Invalid accesstoken"}, DecksterJson.Options, cancellationToken);
            socket.Dispose();
            return null;
        }

        // 1. Send player data
        var playerData = new PlayerData
        {
            PlayerId = user.Id,
            Name = user.Name
        };
        await stream.SendJsonAsync(playerData, DecksterJson.Options, cancellationToken);

        
        // 2. Get client port
        var clientPort = await stream.ReceiveJsonAsync<ClientPortMessage>(DecksterJson.Options, cancellationToken);

        if (clientPort == null)
        {
            _logger.LogError("Did not get client port.");
            await stream.SendJsonAsync(new ServerHelloFailure {ErrorMessage = "Invalid client port"}, DecksterJson.Options, cancellationToken);
            socket.Dispose();
            return null;
        }

        // 2. Connect to client
        var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await clientSocket.ConnectAsync(endpoint.Address, clientPort.Port, cancellationToken);

        var clientStream = new NetworkStream(clientSocket);

        
        // 3. Done
        return new DecksterCommunicator(stream, clientStream, playerData);
    }

    public void Dispose()
    {
        foreach (var communicator in _communicators)
        {
            try
            {
                communicator.OnDisconnected -= OnDisconnected;
                communicator.Dispose();
            }
            catch (ObjectDisposedException)
            {
                // ¯\_(ツ)_/¯
            }
            catch
            {
                _logger.LogError($"Could not dispose communicator {communicator}", communicator.PlayerData.Name);
            }
        }
        GC.SuppressFinalize(this);
    }
}

