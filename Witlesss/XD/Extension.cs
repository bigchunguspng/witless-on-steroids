using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;using System.Text.RegularExpressions;
using static Witlesss.XD.LetterCaseMode;

namespace Witlesss.XD
{
    public static class Extension
    {
        private static readonly Regex Column = new("[:;^Жж]"), Comma = new("[.юб]");
        public  static readonly Regex PngJpg = new("(.png)|(.jpg)"), EmojiRegex = new (REGEX_EMOJI);
        private static readonly Regex Errors = new(@"One or more errors occurred. \((\S*(\s*\S)*)\)");
        
        public static readonly Random Random = new();

        public static int AssumedResponseTime(int initialTime, string text)
        {
            if (text == null) return initialTime;
            return Math.Min(text.Length, 120) * 25;
        }

        public static string ToRandomLetterCase(this string text) => ToLetterCase(text, RandomLetterCase());
        public static string ToLetterCase(this string text, LetterCaseMode mode) => mode switch
        {
            Lower    => text.ToLower(),
            Upper    => text.ToUpper(),
            Sentence => char.ToUpper(text[0]) + text[1..].ToLower(),
            _        => text
        };

        private static LetterCaseMode RandomLetterCase() => Random.Next(8) switch
        {
            < 5 => Lower,
            < 7 => Sentence,
            _   => Upper
        };

        public static string Truncate(this string s, int length) => s.Length > length ? s[..(length - 1)] + "…" : s;

        public static bool HasIntArgument(this string text, out int value)
        {
            value = 0;
            var words = text.Split();
            return words.Length > 1 && int.TryParse(words[1], out value);
        }
        public static bool HasDoubleArgument(this string text, out double value)
        {
            value = 0;
            var words = text.Split();
            return words.Length > 1 && double.TryParse(words[1].Replace('.', ','), out value);
        }

        public static bool IsTimeSpan(this string arg, out TimeSpan span)
        {
            span = TimeSpan.Zero;
            arg = arg.TrimStart('-');

            if (!Regex.IsMatch(arg, @"^(\d+[:;^Жж])?\d+([,.юб]\d+)?$")) return false;
            
            string s = Comma.Replace(Regex.Match(arg, @"\d+([,.юб]\d+)?$").Value, ",");
            string m = Column.Replace(Regex.Match(arg, @"^\d+[:;^Жж]").Value, "");
            
            if (double.TryParse(s, out double seconds)) span  = TimeSpan.FromSeconds(seconds);
            if (double.TryParse(m, out double minutes)) span += TimeSpan.FromMinutes(minutes);

            return true;
        }
        
        public static string FormatDouble(double d) => d.ToString(CultureInfo.InvariantCulture);

        public static string Quote(string s) => $@"""{s}""";

        public static string UniquePath(string path, bool extra = false)
        {
            var cd = true;
            var directory = Path.GetDirectoryName(path);
            var extension = Path.GetExtension(path);

            while (File.Exists(path) || extra)
            {
                cd = false;
                var name  = Path.GetFileNameWithoutExtension(path) ?? "";

                int index = name.LastIndexOf('_');
                if (index > 0 && int.TryParse(name.AsSpan(index).TrimStart('_'), out int number))
                {
                    index++;
                    number++;
                    name = name[..index] + number;
                }
                else
                    name += "_0";

                path = $@"{directory}\{name}{extension}";
                extra = false;
            }
            if (cd) Directory.CreateDirectory(directory);
            
            return path;
        }
        public static string ValidFileName(string text, char x = '_')
        {
            var chars = Path.GetInvalidFileNameChars();
            return chars.Aggregate(text, (current, c) => current.Replace(c, x));
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source) action(element);
        }

        public static string RemoveExtension(this string path) => path.Remove(path.LastIndexOf('.'));

        public static string ShortID(string fileID) => fileID.Remove(62).Remove(2, 44);
        public static string ExtensionFromID(string id) => ExtensionsIDs[id.Remove(2)];
        public static MediaType MediaTypeFromID(string id) => MediaTypes[id.Remove(2)];

        private static readonly Dictionary<string, string> ExtensionsIDs = new()
        {
            {"BA", ".mp4"}, {"Cg", ".mp4"}, // Cg - animation
            {"CQ", ".mp3"}, {"Aw", ".mp3"}, // Aw - voice message / ogg
            {"BQ", ".wav"}, {"DQ", ".mp4"}, // DQ - video message
            {"Ag", ".jpg"}, {"CA", ".webm"} // CA - stickers
        };
        private static readonly Dictionary<string, MediaType> MediaTypes = new()
        {
            {"BA", MediaType.Movie}, {"DQ", MediaType.Round}, {"Cg", MediaType.Video}, {"CA", MediaType.Video},
            {"Aw", MediaType.Audio}, {"BQ", MediaType.Audio}, {"CQ", MediaType.Audio}, {"Ag", MediaType.Video}
        };

        public static string SET_FREQUENCY_RESPONSE(int interval)
        {
            string a = XDDD(Texts.SET_FREQUENCY_RESPONSE);
            if (interval % 10 > 4 || interval % 10 == 0 || interval is > 10 and < 15)
                a = $"{a} каждые {interval} сообщений";
            else if (interval % 10 > 1)
                a = $"{a} каждые {interval} сообщения";
            else if (interval == 1)
                a = $"{a} каждое ваше сообщение";
            else
                a = $"{a} каждое {interval} сообщение";
            return a;
        }

        private static readonly Regex _lat = new(@"[A-Za-z]+");
        private static readonly Regex _cyr = new(@"[А-я]+");

        public static bool IsMostlyCyrillic(string text)
        {
            return _cyr.Matches(text).Count > _lat.Matches(text).Count;
        }
        
        private static readonly Regex _ukrD = new(@"[ієїґ]");
        private static readonly Regex _rusD = new(@"[ыэъё]");
        private static readonly Regex _ukrM = new(@"[авдж]");
        private static readonly Regex _rusM = new(@"[еоть]");

        public static bool LooksLikeUkrainian(string text, out bool sure)
        {
            var u = _ukrD.Matches(text).Count;
            var r = _rusD.Matches(text).Count;

            sure = u > 0 || text.Length > 80;
            return u == r ? _ukrM.Matches(text).Count > _rusM.Matches(text).Count : u > r;
        }

        public static string XDDD(string s) => $"{Pick(RANDOM_EMOJI)} {s}";
        public static string Pick(string[] responses) => responses[Random.Next(responses.Length)];

        public static readonly string[] FILE_TOO_BIG_RESPONSE =
        {
            "пук-среньк...", "много весит 🥺", "тяжёлая штука 🤔", "ого, какой большой 😯", "какой тяжёлый 😩"
        };
        public static readonly string[] UNKNOWN_CHAT_RESPONSE =
        {
            "ты кто?", "я тебя не знаю чувак 😤", "сними маску, я тебя не узнаю", "а ты кто 😲", "понасоздают каналов... 😒"
        };
        public static readonly string[] NOT_ADMIN_RESPONSE =
        {
            "ты не админ 😎", "ты не админ чувак 😒", "попроси админа", "у тебя нет админки 😎", "будет админка - приходи"
        };
        public static readonly string[] I_FORGOR_RESPONSE =
        {
            "Сорян, не помню", "Сорян, не помню такого", "Забыл уже", "Не помню", "Я бы скинул, но уже потерял её"
        };
        public static readonly string[] PLS_WAIT_RESPONSE =
        {
            "жди 😎", "загрузка пошла 😮", "✋ ща всё будет", "еду скачивать музон 🛒", "принял👌"
        };

        private static readonly string[] RANDOM_EMOJI =
        {
            "🔥✍️", "🪵", "😈", "😎", "💯", "📦", "⚙", "🪤", "💡", "🧨", "🫗", "🌭", "☝️",
            "🍒", "🧄", "🍿", "😭", "🪶", "✨", "🍻", "👌", "💀", "🎳", "🗿", "🔧", "🎉"
        };
        public static readonly string[] FAIL_EMOJI_1 = { "🤣", "😎", "🥰", "☺️", "💀", "😤", "😩" };
        public static readonly string[] FAIL_EMOJI_2 = { "😵", "😧", "😨", "😰", "😮", "😲", "💀" };

        public static string GetRandomASCII()
        {
            var files = GetFiles(ASCII_FOLDER);
            return File.ReadAllText(files[Random.Next(files.Length)]);
        }

        public static string FileSize(string path) => FileSize(SizeInBytes(path));
        public static string FileSize(long  bytes)
        {
            long kbs = bytes / 1024;
            return kbs switch { < 1 => bytes + " байт", _ => kbs + " КБ" };
        }

        public static string FixedErrorMessage (string s)
        {
            var match = Errors.Match(s);
            return match.Success ? match.Groups[1].Value : s;
        }

        public static long SizeInBytes         (string path) => new FileInfo(path).Length;
        public static bool FileEmptyOrNotExist (string path) => !File.Exists(path) || SizeInBytes(path) == 0;
        public static void CreateFilePath      (string path) => Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");

        public static FileInfo[] GetFilesInfo  (string path)
        {
            Directory.CreateDirectory(path);
            return new  DirectoryInfo(path).GetFiles();
        }
        
        public static string[]   GetFiles      (string path)
        {
            Directory.CreateDirectory(path);
            return Directory.GetFiles(path);
        }

        public static void ClearTempFiles()
        {
            var path = TEMP_FOLDER;
            var options = new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 3 };
            if (!Directory.Exists(path)) return;
            try
            {
                var x = Directory.GetFiles(path, "*", options).Length;
                Directory.Delete(path, true);
                Log($"DEL TEMP >> {x} FILES!", ConsoleColor.Yellow);
            }
            catch (Exception e)
            {
                LogError("CAN'T DEL TEMP >> " + e.Message);
            }
        }
    }
}