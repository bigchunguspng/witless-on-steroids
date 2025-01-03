using System.Runtime.CompilerServices;

namespace Witlesss.Services.Technical;

public static class Telemetry
{
    private static readonly string[] _buffer = new string[32];

    private static int _head;

    private static void MoveHead()
    {
        if (++_head < 32) return;

        Write();
        _head &= 0b00011111; // same as % 32
    }

    public static void LogCommand
        (long chat, string? text)
        => Log(chat, $"{text}");

    public static void LogInline
        (long chat, string? text)
        => Log(chat, $"{Bot.Username} {text}");

    public static void LogAuto
        (long chat, byte chance, string? text)
        => Log(chat, $"[auto, {chance,3}%] {text}");

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void Log(long chat, string? text)
    {
        _buffer[_head] = $"[{DateTime.Now:MM'/'dd' 'HH:mm:ss.fff} | ..{chat.ToString()[^4..]}] >> {text}";
        Print($"[{_head}] - {_buffer[_head]}");
        MoveHead();
    }

    public static void Write
        () => File.AppendAllLines(File_Log, _buffer.Take(_head));
}