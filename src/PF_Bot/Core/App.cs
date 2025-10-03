using System.Globalization;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Main.Text.Core;
using PF_Bot.Features_Web.Boards.Core;
using PF_Bot.Features_Web.Reddit.Core;
using PF_Bot.Routing_New.Routers;
using PF_Bot.Routing;
using PF_Bot.Telegram;
using PF_Bot.Terminal;

namespace PF_Bot.Core;

public static class App
{
    public  static Bot Bot = null!;

    public  static readonly MessageQueue FunnyMessages = new();

    private static readonly Lazy   <RedditApp>          Reddit_Lazy = new();
    public  static                  RedditApp Reddit => Reddit_Lazy.Value;

    private static readonly Lazy<BoardService>         Chan4_Lazy = new();
    public  static               BoardService Chan4 => Chan4_Lazy.Value;
    private static readonly Lazy<PlankService>         Chan2_Lazy = new();
    public  static               PlankService Chan2 => Chan2_Lazy.Value;

    public static bool LoggedIntoReddit => Reddit_Lazy.IsValueCreated;

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
        if (LoggedIntoReddit) Reddit.SaveExcluded();
    }
}