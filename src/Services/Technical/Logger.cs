using Spectre.Console;

namespace Witlesss.Services.Technical
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

        public static void LogError(string message) => Log(message, LogLevel.Error, 1);
        public static void LogDebug(string message) => Log(message, LogLevel.Debug, 8);

        /// <summary>
        /// Color to number <a href='https://spectreconsole.net/appendix/colors'>cheat sheet</a>
        /// </summary>
        public static void Log(string message, LogLevel level = LogLevel.Info, int color = 7)
        {
            var c = GetLevelChar (level);
            var s = GetLevelColor(level);
            var m = message.EscapeMarkup();
            AnsiConsole.MarkupLine($"[8]{DateTime.Now:MM'/'dd' 'HH:mm:ss.fff}[/] [{s}]{c}[/] [{color}]{m}[/]");
        }

        private static char GetLevelChar(LogLevel level) => level switch
        {
            LogLevel.Debug => 'D',
            LogLevel.Info  => '.',
            LogLevel.Error => '!',
            _              => '?'
        };

        private static int GetLevelColor(LogLevel level) => level switch
        {
            LogLevel.Debug => 3,
            LogLevel.Info  => 7,
            LogLevel.Error => 9,
            _              => 7
        };
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Error
    }
}