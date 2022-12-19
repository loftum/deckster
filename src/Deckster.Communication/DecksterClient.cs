using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Deckster.Communication.Handshake;

namespace Deckster.Communication;

public static class DecksterClient
{
    public static async Task<DecksterCommunicator> ConnectAsync(string host, int port, string accessToken, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        // Timeout: 5 PI seconds
        cts.CancelAfter(TimeSpan.FromSeconds(5 * Math.PI));
        
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Find ip using DNS
        var entry = await Dns.GetHostEntryAsync(host, cts.Token);
        var address = entry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        if (address == null)
        {
            throw new Exception($"Could not connect to '{host}'. No suitable address.");
        }
        
        // Connect to server
        await socket.ConnectAsync(address, port, cts.Token);
        var stream = new NetworkStream(socket);

        // Listen for connections from server
        var listener = new TcpListener(IPAddress.Any, 0);
        listener.Start();
        var localEndpoint = (IPEndPoint) listener.LocalEndpoint;
        var localPort = localEndpoint.Port;
        
        
        // Handshake
        // 1. Pass accesstoken
        var hello = new ClientHelloMessage
        {
            AccessToken = accessToken
        };
        await stream.SendJsonAsync(hello, DecksterJson.Options, cts.Token);
        
        var response = await stream.ReceiveJsonAsync<ServerHelloMessage>(DecksterJson.Options, cts.Token);
        switch (response)
        {
            case null:
                throw new Exception("Handshake error. Server response is null.");
            case ServerHelloFailure failure:
                throw new Exception($"Could not connect: '{failure.ErrorMessage}'");
            case PlayerData playerData:
                var clientPort = new ClientPortMessage {Port = localPort};
                await stream.SendJsonAsync(clientPort, DecksterJson.Options, cts.Token);
                // Wait for server to connect
                var readSocket = await listener.AcceptSocketAsync(cts.Token);
                listener.Stop();
                return new DecksterCommunicator(stream, new NetworkStream(readSocket), playerData);
            default:
                throw new Exception($"Handshake error. '{JsonSerializer.Serialize<object>(response, DecksterJson.Options)}'");
        }
    }
}