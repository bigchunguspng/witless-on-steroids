using System.Globalization;
using PF_Bot.Features_Aux.Packs;
using PF_Bot.Features_Main.Text.Core;
using PF_Bot.Features_Web.Boards.Core;
using PF_Bot.Features_Web.Manga;
using PF_Bot.Features_Web.Reddit.Core;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Messages;
using PF_Bot.Terminal;
using PF_Tools.Graphics;
using PF_Tools.ProcessRunning;

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

    private static readonly Lazy<TCB_Scans_Client>       TCB_Lazy = new();
    public  static               TCB_Scans_Client TCB => TCB_Lazy.Value;

    public static bool LoggedIntoReddit => Reddit_Lazy.IsValueCreated;

    public static async Task Run(string? args)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        Config.ReadFromFile();

        EmojiTool.Directory_EmojiPNGs = Dir_Emoji;
        YtDlp.File_Cookies = File_Cookies;

        Bot = await Bot.Create
        (
            args == null ? new  MessageRouter_Default(Registry. CommandHandlers) : new  MessageRouter_Skip(),
            args == null ? new CallbackRouter_Default(Registry.CallbackHandlers) : new CallbackRouter_Skip()
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
        BigBrother.Log_EXIT();
        BigBrother.Write();

        PackManager.Bakas_SaveDirty();
        if (LoggedIntoReddit) Reddit.SaveExcluded();
    }

    // CLEAN

    public static void ClearTempFiles()
    {
        var options = new EnumerationOptions { RecurseSubdirectories = true };
        ClearDirectory(Dir_Temp,    "*",      options);
        ClearDirectory(Dir_Fuse, "del*.json", options);
    }

    private static void ClearDirectory(FilePath path, string pattern, EnumerationOptions options)
    {
        if (path.DirectoryExists == false) return;

        var files = Directory.GetFiles(path, pattern, options);
        if (files.Length == 0) return;

        try
        {
            var onePunch = options.RecurseSubdirectories && pattern is "*";
            if (onePunch) Directory.Delete(path, true);
            else files.ForEach(File.Delete);

            Print($"CLEAR [{path}] >> {files.Length} FILES!", ConsoleColor.Yellow);
        }
        catch (Exception e)
        {
            Print($"CAN'T CLEAR [{path}] >> {e.Message}", ConsoleColor.Red);
        }
    }
}