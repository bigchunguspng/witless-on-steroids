using System.Globalization;
using PF_Bot.Core.Internet.Boards;
using PF_Bot.Core.Internet.Reddit;
using PF_Bot.Core.Text;
using PF_Bot.Routing_New.Routers;
using PF_Bot.Routing;
using PF_Bot.Telegram;
using PF_Bot.Terminal;

namespace PF_Bot.Core;

public static class App
{
    public  static Bot  Bot              = null!;
    public  static bool LoggedIntoReddit = false;

    private static readonly Lazy<BoardService> _chan4 = new();
    public  static               BoardService   Chan4 => _chan4.Value;
    private static readonly Lazy<PlankService> _chan2 = new();
    public  static               PlankService   Chan2 => _chan2.Value;

    public static async Task Run(string? args)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        Config.ReadFromFile();

        EmojiTool.Directory_EmojiPNGs = Dir_Emoji;

        Bot = await Bot.Create
        (
            args == null ? new CommandRouter() : new Skip(),
            args == null ? new CallbackRouter_Default() : new CallbackRouter_Skip()
        );

        ClearTempFiles();

        var tg_HandleUpdates = args != "!";
        if (tg_HandleUpdates)
            Bot.StartListening();

        PackManager.StartAutoSaveThread(TimeSpan.FromMinutes(2));

        Console.CancelKeyPress              += (_, _) => SaveAndExit();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => SaveAndExit();

        AdminConsole.Start();
    }

    private static void SaveAndExit()
    {
        Print("На выход…", ConsoleColor.Yellow);
        Telemetry.Log_EXIT(Bot.Me);
        Telemetry.Write();

        PackManager.Bakas_SaveDirty();
        if (LoggedIntoReddit) RedditTool.Instance.SaveExcluded();
    }
}