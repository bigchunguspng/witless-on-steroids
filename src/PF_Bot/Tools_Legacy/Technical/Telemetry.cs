using PF_Bot.Telegram;

namespace PF_Bot.Tools_Legacy.Technical;

public static class Telemetry
{
    private static readonly FileLogger_Batch _logger = new (File_Log);

    public static void LogCommand
        (long chat, string? text)
        => Log(chat, $"{text}");

    public static void LogAutoCommand
        (long chat, string? text)
        => Log(chat, $"[auto] {text}");

    public static void LogInline
        (long chat, string? text)
        => Log(chat, $"{Bot.Username} {text}");

    public static void LogAuto
        (long chat, byte chance, string? text)
        => Log(chat, $"[auto, {chance,3}%] {text}");

    private static void Log(long chat, string? text)
    {
        var chat_Last4Digits = chat.ToString()[^4..];
        _logger.Log($"[{DateTime.Now:MM'/'dd' 'HH:mm:ss.fff} | ..{chat_Last4Digits}] >> {text}");
    }

    public static void Write() => _logger.Write();
}