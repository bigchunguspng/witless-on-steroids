using System.Runtime.CompilerServices;
using Spectre.Console;

namespace PF_Tools.Logging;

public static class ConsoleLogger
{
    // PRINT

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void Print
        (string message) =>
        Console.WriteLine(message);

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void Print
        (string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    // LOG (MORE FANCY)

    public static void LogError
        (string message, LogColor color = LogColor.Maroon)
        => Log(message, LogLevel.Error, color);

    public static void LogDebug
        (string message, LogColor color = LogColor.Grey)
        => Log(message, LogLevel.Debug, color);

    public static void Log
    (
        string message,
        LogLevel level = LogLevel.Info,
        LogColor color = LogColor.Silver
    )
    {
        var now     = DateTime.Now;
        var icon    = level.GetCharIcon();
        var icon_CC = level.GetDefaultColor().ToConsoleColor();
        var text_CC = color                  .ToConsoleColor();
        lock (typeof(ConsoleLogger))
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"{now:MM'/'dd' 'HH:mm:ss.fff} ");
            Console.ForegroundColor = icon_CC;
            Console.Write($"{icon} ");
            Console.ForegroundColor = text_CC;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    //

    private static char GetCharIcon
        (this LogLevel level) => level switch
    {
        LogLevel.Debug => 'D',
        LogLevel.Info  => '#',
        LogLevel.Error => '!',
        _              => '?',
    };

    private static LogColor GetDefaultColor
        (this LogLevel level) => level switch
    {
        LogLevel.Debug => LogColor.Olive,
        LogLevel.Info  => LogColor.Silver,
        LogLevel.Error => LogColor.Red,
        _              => LogColor.Silver,
    };
    
    private static ConsoleColor ToConsoleColor
        (this LogColor color) => color switch
    {
        LogColor.Maroon  => ConsoleColor.DarkRed,
        LogColor.Olive   => ConsoleColor.DarkYellow,
        LogColor.Silver  => ConsoleColor.Gray,
        LogColor.Grey    => ConsoleColor.DarkGray,
        LogColor.Red     => ConsoleColor.Red,
        LogColor.Lime    => ConsoleColor.Green,
        LogColor.Yellow  => ConsoleColor.Yellow,
        LogColor.Blue    => ConsoleColor.Blue,
        LogColor.Fuchsia => ConsoleColor.Magenta,
        _                => ConsoleColor.White,
    };
}

public enum LogLevel
{
    Debug,
    Info,
    Error,
}

/// See <a href='https://spectreconsole.net/appendix/colors'>cheat sheet</a>
public enum LogColor
{
    Maroon   =  1,
    Olive    =  3,
    Silver   =  7,
    Grey     =  8,
    Red      =  9,
    Lime     = 10,
    Yellow   = 11,
    Blue     = 12,
    Fuchsia  = 13,
}