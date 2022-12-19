namespace Deckster.Server;

public static class Log
{
    public static ILoggerFactory Factory { get; }
    
    static Log()
    {
        Factory = LoggerFactory.Create(b => b.AddConsole());
    }
}