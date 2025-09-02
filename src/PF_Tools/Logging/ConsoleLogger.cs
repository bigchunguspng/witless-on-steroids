using Spectre.Console;

namespace PF_Tools.Logging;

public static class ConsoleLogger
{
    // PRINT

    public static void Print
        (string message) =>
        Console.WriteLine(message);

    public static void Print
        (string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Print(message);
        Console.ResetColor();
    }

    // LOG (MORE FANCY)

    public static void LogError
        (string message) => Log(message, LogLevel.Error, LogColor.Maroon);

    public static void LogDebug
        (string message) => Log(message, LogLevel.Debug, LogColor.Grey);

    public static void Log
    (
        string message,
        LogLevel level = LogLevel.Info,
        LogColor color = LogColor.Silver
    )
    {
        var icon  = level.GetCharIcon();
        var style = level.GetDefaultColor();
        var log = $"[8]{DateTime.Now:MM'/'dd' 'HH:mm:ss.fff}[/] "
                + $"[{style}]{icon}[/] "
                + $"[{color}]{message.EscapeMarkup()}[/]";
        AnsiConsole.MarkupLine(log);
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