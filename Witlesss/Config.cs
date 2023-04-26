using System;
using System.Text.RegularExpressions;

namespace Witlesss // ReSharper disable InconsistentNaming
{
    public static class Config
    {
        public static string TelegramToken { get; private set; }
        public static string RedditAppID   { get; private set; }
        public static string RedditToken   { get; private set; }
        public static string BOT_USERNAME  { get; private set; }
        public static string ArtLocation   { get; private set; }
        public static string FontBold      { get; private set; }
        public static string FontRegular   { get; private set; }

        private const string path = "config.txt";

        public static void ReadFromFile()
        {
            var file = File.ReadAllText(path);
            GetValue(@"t\S*g\S*token\s+=\s+(\S+)",    s => TelegramToken = s, "telegram-token"      );
            GetValue(   @"r\S*app\S*\s+=\s+(\S+)",    s => RedditAppID   = s, "reddit-app-id"       );
            GetValue( @"r\S*token\S*\s+=\s+(\S+)",    s => RedditToken   = s, "reddit-refresh-token");
            GetValue(   @"a\S*art\S*\s+=\s+""(.+)""", s => ArtLocation   = s, "album-art"           );
            GetValue(  @"b\S*font\S*\s+=\s+""(.+)""", s => FontBold      = s,            "bold-font");
            GetValue(  @"r\S*font\S*\s+=\s+""(.+)""", s => FontRegular   = s,         "regular-font");

            void GetValue(string pattern, Action<string> action, string prop)
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var match = regex.Match(file);
                if (match.Success) action(match.Groups[1].Value);
                else
                {
                    LogError($"Please add {prop} to {path} and restart the app.");
                    Console.ReadKey();
                }
            }
        }
        public static void SetBotUsername(string username) => BOT_USERNAME = $"@{username.ToLower()}";
    }
}