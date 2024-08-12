using System;
using System.Text.RegularExpressions;

namespace Witlesss // ReSharper disable InconsistentNaming
{
    public static class Config
    {
        public static string TelegramToken { get; private set; } = default!;
        public static string RedditAppID   { get; private set; } = default!;
        public static string RedditToken   { get; private set; } = default!;
        public static long   AdminID       { get; private set; }

        public static void ReadFromFile()
        {
            var file = File.ReadAllText(File_Config);
            GetValue(@"t\S*g\S*token\s+=\s+(\S+)",    s => TelegramToken = s, "telegram-token"      );
            GetValue(   @"r\S*app\S*\s+=\s+(\S+)",    s => RedditAppID   = s, "reddit-app-id"       );
            GetValue( @"r\S*token\S*\s+=\s+(\S+)",    s => RedditToken   = s, "reddit-refresh-token");
            GetValue(  @"\S*admin\S*\s+=\s+(\S+)",    s => AdminID       = GetLong(s),    "admin-id");

            void GetValue(string pattern, Action<string> action, string prop)
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var match = regex.Match(file);
                if (match.Success) action(match.Groups[1].Value);
                else
                {
                    LogError($"Please add {prop} to {File_Config} and restart the app.");
                    Console.ReadKey();
                }
            }
        }

        private static long GetLong(string s) => long.TryParse(s, out var result) ? result : 0;
    }
}