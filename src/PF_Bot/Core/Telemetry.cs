using Telegram.Bot.Types;

namespace PF_Bot.Core;

public static class Telemetry
{
    private static readonly FileLogger_Batch _logger = new (File_Log);

    public static void LogCommand
        (long chat, string? text)
        => Log(chat, $"-> {text}");

    public static void LogCallback
        (long chat, string? text)
        => Log(chat, $"-> OK   *{text}");

    public static void LogAutoCommand
        (long chat, string? text)
        => Log(chat, $"[auto] {text}"); // -V @_30% /scale 0.5 > /nuke > /damn 15

    public static void LogInline
        (long chat, string? text)
        => Log(chat, $"-- OK   {App.Bot.Username} {text}");

    public static void LogAuto
        (long chat, byte chance, string? text)
        => Log(chat, $"[auto, {chance,3}%] {text}"); // PP @150% /toplarg text

    public static void Log_START
        (User me)
        => Log(" START", $">> @{me.Username!.ToLower()} | {me.FirstName}");

    public static void Log_EXIT
        (User me)
        => Log("  EXIT", $">> @{me.Username!.ToLower()} | {me.FirstName}\n");

    private static void Log(long chat, string? text)
    {
        var chat_Last5Digits = $"~{chat.ToString("#00000").AsSpan(^5)}";
        Log(chat_Last5Digits, text);
    }

    private static void Log(string id_6char, string? text)
    {
        _logger.Log($"{DateTime.Now:MM'/'dd' 'HH:mm:ss.fff} | {id_6char} {text}");
    }

    public static void Write() => _logger.Write();
}