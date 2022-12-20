using System.Net;
using System.Net.Sockets;
using Deckster.Communication.Handshake;

namespace Deckster.Communication;

public static class DecksterClient
{
    public static async Task<DecksterCommunicator> ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        var (host, port, accessToken, path) = Extract(uri);

        // Timeout: 5 PI seconds
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5 * Math.PI));
        
        var address = await GetIpAddressAsync(host, cts.Token);
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // Connect to server
        await socket.ConnectAsync(address, port, cts.Token);
        var stream = new NetworkStream(socket);

        // Listen for connections from server
        var listener = new TcpListener(IPAddress.Any, 0);

        try
        {
            listener.Start();
            var localEndpoint = (IPEndPoint) listener.LocalEndpoint;
            var localPort = localEndpoint.Port;

            var hello = new ConnectRequest
            {
                AccessToken = accessToken,
                ClientPort = localPort,
                Path = path
            };
            await stream.SendJsonAsync(hello, DecksterJson.Options, cts.Token);

            var response = await stream.ReceiveJsonAsync<ConnectResponse>(DecksterJson.Options, cts.Token);

            if (response == null)
            {
                throw new Exception("Handshake error. Server response is null.");
            }
            
            switch (response.StatusCode)
            {
                case 200:
                    var readSocket = await listener.AcceptSocketAsync(cts.Token);
                    listener.Stop();
                    var readStream = new NetworkStream(readSocket);
                    var communicator = new DecksterCommunicator(readStream, stream, response.PlayerData);
                    return communicator;
                default:
                    throw new Exception($"Could not connect: '{response.StatusCode}: {response.Description}'");
            }
        }
        catch
        {
            socket.Dispose();
            throw;
        }
        finally
        {
            listener.Stop();
        }
    }

    private static async ValueTask<IPAddress> GetIpAddressAsync(string host, CancellationToken cancellationToken)
    {
        // Find ip using DNS
        var entry = await Dns.GetHostEntryAsync(host, cancellationToken);
        var address = entry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        if (address == null)
        {
            throw new Exception($"Could not connect to '{host}'. No suitable address.");
        }

        return address;
    }

    private static (string host, int port, string accessToken, string path) Extract(Uri uri)
    {
        var host = uri.Host;
        var port = uri.Port < 0 ? DecksterConstants.TcpPort : uri.Port;
        var accessToken = uri.UserInfo;
        var path = uri.AbsolutePath;
        return (host, port, accessToken, path);
    }
}