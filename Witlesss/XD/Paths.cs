﻿using System.IO;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace Witlesss.XD;

/*
    Nobody:
    Working directory structure:

    DB/
        Backup/
            2024-03-14/
                pack--4147302158.json [dic]
        Board/
            a - 2023-12-22 02.41.json [list]
        Chat/
            chats.json
            bans.json
            pack--1001699898486.json [dic]
        Fuse/
            any-name-possible.json [dic]
        History/
            -1001541923355/
                Тестировка-1705243895355.json [list]
        default.json
        baguette.json

    Pics/
        -1001539756197/
            Ag0ygAACZ8YxG55ScE-D.jpg

    Static/
        ASCII/
        Emoji/
        Fonts/
        Water/
        art.jpg
        voice.ogg

    config.txt
    reddit-posts.json
    spam.txt
*/

public static class Paths
{
    public const string Dir_DB = "DB", Dir_Pics = "Pics", Dir_Static = "Static", Dir_Temp = "Temp";

    public const string Prefix_Pack = "pack";

    public const string File_Config      = "config.txt";
    public const string File_Spam        = "spam.txt";
    public const string File_RedditPosts = "reddit-posts.json";

    public static string Dir_Backup  { get; } = Path.Combine(Dir_DB, "Backup");
    public static string Dir_Board   { get; } = Path.Combine(Dir_DB, "Board");
    public static string Dir_Chat    { get; } = Path.Combine(Dir_DB, "Chat");
    public static string Dir_Fuse    { get; } = Path.Combine(Dir_DB, "Fuse");
    public static string Dir_History { get; } = Path.Combine(Dir_DB, "History");

    public static string File_Chats { get; } = Path.Combine(Dir_Chat, "chats.json");
    public static string File_Bans  { get; } = Path.Combine(Dir_Chat,  "bans.json");

    public static string File_DefaultTexts { get; } = Path.Combine(Dir_DB,  "default.json");
    public static string File_Baguette     { get; } = Path.Combine(Dir_DB, "baguette.json");

    public static string Dir_ASCII { get; } = Path.Combine(Dir_Static, "ASCII");
    public static string Dir_Emoji { get; } = Path.Combine(Dir_Static, "Emoji");
    public static string Dir_Fonts { get; } = Path.Combine(Dir_Static, "Fonts");
    public static string Dir_Water { get; } = Path.Combine(Dir_Static, "Water");

    public static string File_DefaultAlbumCover   { get; } = Path.Combine(Dir_Static, "art.jpg");
    public static string File_DefaultVoiceMessage { get; } = Path.Combine(Dir_Static, "voice.ogg");
}