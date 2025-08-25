// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace PF_Bot.Backrooms.Static;

/*
    Nobody:
    Working directory structure:

    DB/
        Alias/
            Peg/
                vintage.txt
            Im/
                bb.txt
        Chat/
            pack--1001699898486.json     [pack] <-- /fuse by id
        Chat.Backup/
            2024-03-14/
                pack--4147302158.json    [pack]
        Fuse/
            -1001541923355/
                private_3D_AF.json       [pack] <-- /fuse ! info
            any-name-possible.json       [pack] <-- /fuse   info
        History/
            -1001541923355/
                KINGPIN-funny.json       [list] <-- /fuse * info
            that-funny-file-1.json       [list] <-- /fuse @ info
        History.Board/
            2024-08-21 a.270089129.json  [list] <-- /boards info
        History.Plank/
            2024-08-21 a.7819159.json    [list] <-- /planks info

        chats.json
        GIFs.txt
        sounds.txt

    Pics/
        -1001539756197/
            AgAD5g0AAkFXMUk+3D8D-Meme.webp

    Static/
        ASCII/
        Emoji/
        Fonts/
            Fallback/
        Manual/
        Water/

        2chan.html
        4chan.html
        art.jpg
        texts.json
        voice.ogg

    Temp/
        ...

    config.txt
    errors.txt
    log.txt
    reddit-posts.json
*/

public static class Paths
{
    public const string Dir_DB = "DB", Dir_Pics = "Pics", Dir_Static = "Static", Dir_Temp = "Temp";

    public const string Prefix_Pack = "pack";

    public const string File_Config      = "config.txt";
    public const string File_Errors      = "errors.txt";
    public const string File_Log         = "log.txt";
    public const string File_RedditPosts = "reddit-posts.json";

    public static string Dir_Alias   { get; } = Path.Combine(Dir_DB, "Alias");
    public static string Dir_Chat    { get; } = Path.Combine(Dir_DB, "Chat");
    public static string Dir_Backup  { get; } = Path.Combine(Dir_DB, "Chat.Backup");
    public static string Dir_Fuse    { get; } = Path.Combine(Dir_DB, "Fuse");
    public static string Dir_History { get; } = Path.Combine(Dir_DB, "History");
    public static string Dir_Board   { get; } = Path.Combine(Dir_DB, "History.Board");
    public static string Dir_Plank   { get; } = Path.Combine(Dir_DB, "History.Plank");
    public static string File_Chats  { get; } = Path.Combine(Dir_DB, "chats.json");
    public static string File_GIFs   { get; } = Path.Combine(Dir_DB, "GIFs.txt");
    public static string File_Sounds { get; } = Path.Combine(Dir_DB, "sounds.txt");

    public static string Dir_Alias_Peg { get; } = Path.Combine(Dir_Alias, "Peg");
    public static string Dir_Alias_Im  { get; } = Path.Combine(Dir_Alias, "Im");

    public static string Dir_ASCII  { get; } = Path.Combine(Dir_Static, "ASCII");
    public static string Dir_Emoji  { get; } = Path.Combine(Dir_Static, "Emoji");
    public static string Dir_Fonts  { get; } = Path.Combine(Dir_Static, "Fonts");
    public static string Dir_Manual { get; } = Path.Combine(Dir_Static, "Manual");
    public static string Dir_Water  { get; } = Path.Combine(Dir_Static, "Water");

    public static string Dir_Fonts_Fallback  { get; } = Path.Combine(Dir_Fonts, "Fallback");

    public static string File_2chanHtmlPage       { get; } = Path.Combine(Dir_Static, "2chan.html");
    public static string File_4chanHtmlPage       { get; } = Path.Combine(Dir_Static, "4chan.html");
    public static string File_DefaultAlbumCover   { get; } = Path.Combine(Dir_Static, "art.jpg");
    public static string File_DefaultTexts        { get; } = Path.Combine(Dir_Static, "texts.json");
    public static string File_DefaultVoiceMessage { get; } = Path.Combine(Dir_Static, "voice.ogg");


    public static void ClearTempFiles()
    {
        ClearDirectory(Dir_Temp,    "*",      new EnumerationOptions { RecurseSubdirectories = true });
        ClearDirectory(Dir_Fuse, "del*.json", new EnumerationOptions { RecurseSubdirectories = true });
    }

    private static void ClearDirectory(string path, string pattern, EnumerationOptions options)
    {
        if (!Directory.Exists(path)) return;

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
