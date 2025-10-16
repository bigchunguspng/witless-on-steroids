namespace PF_Tools.Backrooms.Helpers;

public static class HttpClientFactory
{
    private static readonly SocketsHttpHandler _handler = new()
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    };

    public static HttpClient CreateClient
        () => new(_handler, disposeHandler: false);
}