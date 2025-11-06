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
        err.mkd
        log.txt

    Pics/
        Manga/
            one-piece/1162/
                1162 - 001.jpg
        Memes/
            t3_1cibu47.png
        -1001539756197/
            AgAD5g0AAkFXMUk+3D8D-Meme.webp

    Static/
        ASCII/
            xd.txt
        Backs/
            win xp.png
        Emoji/
            1f4af-1f3a6.png
        Fonts/
            Fallback/
                1 Sawarabi Gothic.ttf
            bb-bi.ttf
        Manual/
            44 FFMpeg.html
        Water/
            556 698.png

        2chan.html
        4chan.html
        art.jpg
        fonts-back.png
        texts.json
        voice.ogg

    Temp/
        ...

    config.txt
    reddit-posts.json
*/

public static class Paths
{
    public const string Ext_Pack = ".tgp";

    public static readonly FilePath
        // Top level dirs
        Dir_DB     = "DB",
        Dir_Log    = "Log",
        Dir_Pics   = "Pics",
        Dir_Static = "Static",
        Dir_Temp   = "Temp",
        File_Config      = "config.txt",
        File_RedditPosts = "reddit-posts.json",
        // DB/
        Dir_Alias   = Dir_DB.Combine("Alias"),
        Dir_Chat    = Dir_DB.Combine("Chat"),
        Dir_Fuse    = Dir_DB.Combine("Fuse"),
        Dir_History = Dir_DB.Combine("History"),
        Dir_Board   = Dir_DB.Combine("History.Board"),
        Dir_Plank   = Dir_DB.Combine("History.Plank"),
        File_Chats  = Dir_DB.Combine("chats.json"),
        File_GIFs   = Dir_DB.Combine("GIFs.txt"),
        File_Sounds = Dir_DB.Combine("sounds.txt"),
        // DB/Alias/
        Dir_Alias_Peg = Dir_Alias.Combine("Peg"),
        Dir_Alias_Im  = Dir_Alias.Combine("Im"),
        // Log/
        Dir_Reports = Dir_Log.Combine("Reports"),
        File_Errors = Dir_Log.Combine("err.mkd"),
        File_Log    = Dir_Log.Combine("log.txt"),
        // Pics/
        Dir_Manga       = Dir_Pics.Combine("Manga"),
        Dir_RedditMemes = Dir_Pics.Combine("Memes"),
        // Static/
        Dir_ASCII  = Dir_Static.Combine("ASCII"),
        Dir_Backs  = Dir_Static.Combine("Backs"),
        Dir_Emoji  = Dir_Static.Combine("Emoji"),
        Dir_Fonts  = Dir_Static.Combine("Fonts"),
        Dir_Manual = Dir_Static.Combine("Manual"),
        Dir_Water  = Dir_Static.Combine("Water"),
        File_2chanHtmlPage       = Dir_Static.Combine("2chan.html"),
        File_4chanHtmlPage       = Dir_Static.Combine("4chan.html"),
        File_DefaultAlbumCover   = Dir_Static.Combine("art.jpg"),
        File_TestFontsBackground = Dir_Static.Combine("fonts-back.png"),
        File_DefaultTexts        = Dir_Static.Combine("texts.json"),
        File_DefaultVoiceMessage = Dir_Static.Combine("voice.ogg"),
        // Static/Fonts/
        Dir_Fonts_Fallback = Dir_Fonts.Combine("Fallback");

    //

    public static string GetTempFileName
        (string extension) => Dir_Temp
        .EnsureDirectoryExist()
        .Combine($"{Desert.GetSand(8)}.{extension}")
        .MakeUnique();
}
