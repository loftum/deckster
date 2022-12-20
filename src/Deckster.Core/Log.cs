using Microsoft.Extensions.Logging;

namespace Deckster.Core;

public static class Log
{
    public static ILoggerFactory Factory { get; }
    
    static Log()
    {
        Factory = LoggerFactory.Create(b => b.AddConsole());
    }
}