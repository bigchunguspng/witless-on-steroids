using System;
using System.Text.RegularExpressions;

namespace Witlesss
{
    public static class Config
    {
        public static string TelegramToken { get; private set; }
        public static string RedditAppID   { get; private set; }
        public static string RedditToken   { get; private set; }
        public static string BotUsername   { get; private set; }
        public static string FFMpegPath    { get; private set; }

        private const string path = "config.txt";

        public static void ReadFromFile()
        {
            var file = File.ReadAllText(path);
            GetValue(@"t\S*g\S*token\s+=\s+(\S+)",   s => TelegramToken = s, "telegram-token"      );
            GetValue(   @"r\S*app\S*\s+=\s+(\S+)",   s => RedditAppID   = s, "reddit-app-id"       );
            GetValue( @"r\S*token\S*\s+=\s+(\S+)",   s => RedditToken   = s, "reddit-refresh-token");
            GetValue(@"ff\S*path\S*\s+=\s+""(.+)""", s => FFMpegPath    = s, "ffmpeg-path"         );

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
        public static void SetBotUsername(string username) => BotUsername = $"@{username.ToLower()}";
    }
}