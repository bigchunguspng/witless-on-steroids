namespace PF_Bot.Core // ReSharper disable InconsistentNaming
{
    public static class Config
    {
        public static string TelegramToken { get; private set; } = null!;
        public static string RedditAppID   { get; private set; } = null!;
        public static string RedditToken   { get; private set; } = null!;
        public static string RedditSecret  { get; private set; } = null!;
        public static long[] AdminIDs      { get; private set; } = null!;
        public static long   SoundChannel  { get; private set; }

        public static void ReadFromFile()
        {
            var file = File.ReadAllText(File_Config);
            GetValue(s => TelegramToken = s,             "tg-token");
            GetValue(s => RedditAppID   = s, "reddit-app-id"       );
            GetValue(s => RedditToken   = s, "reddit-refresh-token");
            GetValue(s => RedditSecret  = s, "reddit-secret"       );
            GetValue(s => AdminIDs      = GetLongs(s),   "admin-id");
            GetValue(s => SoundChannel  = GetLong(s),  "sound-chat");

            void GetValue(Action<string> action, string propertyName)
            {
                var match = Regex.Match(file, $@"{propertyName}\s+=\s+(\S+)", RegexOptions.IgnoreCase);
                if (match.Success) action(match.Groups[1].Value);
                else
                {
                    Print($@"CAN'T CONFIGURE BOT | Add ""{propertyName}"" to ""{File_Config}"" and restart the bot.", ConsoleColor.Red);
                    Console.ReadKey();
                }
            }
        }

        private static long GetLong
            (string s) => long.TryParse(s, out var result) ? result : 0;

        private static long[] GetLongs
            (string s) => s.Split(',').Select(x => long.TryParse(x, out var result) ? result : 0).ToArray();
    }
}