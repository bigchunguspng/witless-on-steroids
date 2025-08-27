using Spectre.Console;

namespace PF_Bot.Tools_Legacy.Technical
{
    public static class Logger
    {
        public static void Print(string message) => Console.WriteLine(message);

        public static void Print(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Print(message);
            Console.ResetColor();
        }

        public static void LogError(string message) => Log(message, LogLevel.Error, LogColor.Maroon);
        public static void LogDebug(string message) => Log(message, LogLevel.Debug, LogColor.Grey);

        public static void Log(string message, LogLevel level = LogLevel.Info, LogColor color = LogColor.Silver)
        {
            var c = GetLevelChar (level);
            var s = GetLevelColor(level);
            var m = message.EscapeMarkup();
            AnsiConsole.MarkupLine($"[8]{DateTime.Now:MM'/'dd' 'HH:mm:ss.fff}[/] [{(int)s}]{c}[/] [{color}]{m}[/]");
        }

        private static char GetLevelChar(LogLevel level) => level switch
        {
            LogLevel.Debug => 'D',
            LogLevel.Info  => '#',
            LogLevel.Error => '!',
            _              => '?'
        };

        private static LogColor GetLevelColor(LogLevel level) => level switch
        {
            LogLevel.Debug => LogColor.Olive,
            LogLevel.Info  => LogColor.Silver,
            LogLevel.Error => LogColor.Red,
            _              => LogColor.Silver
        };
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Error
    }

    /// <summary>
    /// See <a href='https://spectreconsole.net/appendix/colors'>cheat sheet</a>
    /// </summary>
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
}