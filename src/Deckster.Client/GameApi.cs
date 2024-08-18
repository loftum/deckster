using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Deckster.Client.Communication;

namespace Deckster.Client;

public class GameApi<TClient>
{
    private readonly Uri _baseUri;
    private readonly string _token;
    private readonly Func<WebSocketDecksterChannel, TClient> _createClient;

    public GameApi(Uri baseUri, string token, Func<WebSocketDecksterChannel, TClient> createClient)
    {
        _baseUri = baseUri;
        _token = token;
        _createClient = createClient;
    }

    public async Task<TClient> CreateAndJoinAsync(CancellationToken cancellationToken = default)
    {
        var gameInfo = await CreateAsync(cancellationToken);
        var client = await JoinAsync(gameInfo.Id, cancellationToken);
        return client;
    }

    public async Task<TClient> JoinAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var channel = await WebSocketDecksterChannel.ConnectAsync(_baseUri, gameId, _token, cancellationToken);
        return _createClient(channel);
    }

    public async Task<GameInfo> CreateAsync(CancellationToken cancellationToken = default)
    {
        var client = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, _baseUri.Append("create"))
        {
            Headers =
            {
                Accept = {MediaTypeWithQualityHeaderValue.Parse("application/json")},
                Authorization = AuthenticationHeaderValue.Parse($"Bearer {_token}")
            }
        };

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    var gameInfo = await response.Content.ReadFromJsonAsync<GameInfo>(DecksterJson.Options, cancellationToken: cancellationToken);
                    return gameInfo;
                }
                default:
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);
                    throw new Exception($"Could not create game:\n{request.Method} {request.RequestUri}\n{(int)response.StatusCode} ({response.StatusCode})\n{body}");
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Could not {request.Method} {request.RequestUri}", e);
        }
    }
}

public static class UriExtensions
{
    public static Uri Append(this Uri uri, string path)
    {
        var builder = new StringBuilder()
            .Append($"{uri.Scheme}://")
            .Append(uri.Authority);
        if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
        {
            builder.Append(uri.AbsolutePath);
        }

        builder.Append($"/{path.TrimStart('/')}");

        return new Uri(builder.ToString());
    }

    public static Uri ToWebSocket(this Uri uri, string path)
    {
        var scheme = uri.Scheme switch
        {
            "http" => "ws",
            "https" => "wss",
            _ => "ws"
        };
        var builder = new StringBuilder($"{scheme}://")
            .Append(uri.Authority);
        if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
        {
            builder.Append(uri.AbsolutePath);
        }

        builder.Append($"/{path.TrimStart('/')}");

        return new Uri(builder.ToString());
    }
}