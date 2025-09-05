// ReSharper disable MemberCanBePrivate.Global

namespace PF_Bot.Core;

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
            -1001699898486.tgp           [pack] <-- /fuse by id
        Fuse/
            -1001541923355/
                private_3D_AF.tgp        [pack] <-- /fuse ! info
            any-name-possible.tgp        [pack] <-- /fuse   info
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

    Log/
        Reports/
            2025-09-11-ffmpeg--1001539756197-BRS0peYTP41.txt
        errors.txt
        log.txt

    Pics/
        Memes/
            t3_1cibu47.png
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
    reddit-posts.json
*/

public static class Paths
{
    public static FilePath Dir_DB     { get; } = "DB";
    public static FilePath Dir_Log    { get; } = "Log";
    public static FilePath Dir_Pics   { get; } = "Pics";
    public static FilePath Dir_Static { get; } = "Static";
    public static FilePath Dir_Temp   { get; } = "Temp";

    public static FilePath File_Config      { get; } = "config.txt";
    public static FilePath File_RedditPosts { get; } = "reddit-posts.json";

    public const string Ext_Pack = ".tgp";

    public static FilePath Dir_Alias   { get; } = Dir_DB.Combine("Alias");
    public static FilePath Dir_Chat    { get; } = Dir_DB.Combine("Chat");
    public static FilePath Dir_Fuse    { get; } = Dir_DB.Combine("Fuse");
    public static FilePath Dir_History { get; } = Dir_DB.Combine("History");
    public static FilePath Dir_Board   { get; } = Dir_DB.Combine("History.Board");
    public static FilePath Dir_Plank   { get; } = Dir_DB.Combine("History.Plank");
    public static FilePath File_Chats  { get; } = Dir_DB.Combine("chats.json");
    public static FilePath File_GIFs   { get; } = Dir_DB.Combine("GIFs.txt");
    public static FilePath File_Sounds { get; } = Dir_DB.Combine("sounds.txt");

    public static FilePath Dir_Reports { get; } = Dir_Log.Combine("Reports");
    public static FilePath File_Log    { get; } = Dir_Log.Combine("log.txt");
    public static FilePath File_Errors { get; } = Dir_Log.Combine("errors.txt");

    public static FilePath Dir_Alias_Peg { get; } = Dir_Alias.Combine("Peg");
    public static FilePath Dir_Alias_Im  { get; } = Dir_Alias.Combine("Im");

    public static FilePath Dir_ASCII  { get; } = Dir_Static.Combine("ASCII");
    public static FilePath Dir_Emoji  { get; } = Dir_Static.Combine("Emoji");
    public static FilePath Dir_Fonts  { get; } = Dir_Static.Combine("Fonts");
    public static FilePath Dir_Manual { get; } = Dir_Static.Combine("Manual");
    public static FilePath Dir_Water  { get; } = Dir_Static.Combine("Water");

    public static FilePath Dir_RedditMemes { get; } = Dir_Pics.Combine("Memes");

    public static FilePath Dir_Fonts_Fallback  { get; } = Dir_Fonts.Combine("Fallback");

    public static FilePath File_2chanHtmlPage       { get; } = Dir_Static.Combine("2chan.html");
    public static FilePath File_4chanHtmlPage       { get; } = Dir_Static.Combine("4chan.html");
    public static FilePath File_DefaultAlbumCover   { get; } = Dir_Static.Combine("art.jpg");
    public static FilePath File_DefaultTexts        { get; } = Dir_Static.Combine("texts.json");
    public static FilePath File_DefaultVoiceMessage { get; } = Dir_Static.Combine("voice.ogg");


    // TODO: MOVE TFO

    public static void ClearTempFiles()
    {
        ClearDirectory(Dir_Temp,    "*",      new EnumerationOptions { RecurseSubdirectories = true });
        ClearDirectory(Dir_Fuse, "del*.json", new EnumerationOptions { RecurseSubdirectories = true });
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
