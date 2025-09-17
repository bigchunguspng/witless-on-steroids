using System.Globalization;
using PF_Bot.Core.Chats;
using PF_Bot.Core.Internet.Reddit;
using PF_Bot.Routing;
using PF_Bot.Telegram;
using PF_Bot.Terminal;

namespace PF_Bot.Core;

public static class App
{
    public  static Bot  Bot              = null!;
    public  static bool LoggedIntoReddit = false;

    public static async Task Run(string? args)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        Config.ReadFromFile();

        EmojiTool.Directory_EmojiPNGs = Dir_Emoji;

        Bot = await Bot.Create(args == null ? new CommandRouter() : new Skip());

        ClearTempFiles();

        var tg_HandleUpdates = args != "!";
        if (tg_HandleUpdates)
            Bot.StartListening();

        ChatManager.StartAutoSaveThread(TimeSpan.FromMinutes(2));

        Console.CancelKeyPress              += (_, _) => SaveAndExit();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => SaveAndExit();

        TerminalUI.Start();
    }

    private static void SaveAndExit()
    {
        Print("На выход…", ConsoleColor.Yellow);
        Telemetry.Log_EXIT(Bot.Me);
        Telemetry.Write();

        ChatManager.Bakas_SaveDirty();
        if (LoggedIntoReddit) RedditTool.Instance.SaveExcluded();
    }
}