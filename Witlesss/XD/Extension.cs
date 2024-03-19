using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using static Witlesss.XD.LetterCaseMode;

namespace Witlesss.XD
{
    public static class Extension
    {
        private static readonly Regex Column = new("[:;^Жж]"), Comma = new("[.юб]");
        public  static readonly Regex PngJpg = new("(.png)|(.jpg)"), EmojiRegex = new (REGEX_EMOJI);
        public  static readonly Regex FFmpeg = new(@"ffmpeg|ffprobe", RegexOptions.IgnoreCase);
        private static readonly Regex Errors = new(@"One or more errors occurred. \((\S*(\s*\S)*)\)");
        
        public static readonly Random Random = new();

        public static int AssumedResponseTime(int initialTime, string text)
        {
            if (text == null) return initialTime;
            return Math.Min(text.Length, 120) * 25;
        }

        public static bool IsOneIn         (int x) => Random.Next(x) == 0;
        public static bool IsFirstOf(int a, int b) => Random.Next(a + b) < a;

        public static int    RandomInt   (int    min, int    max) => Random.Next(min, max + 1);
        public static double RandomDouble(double min, double max)
        {
            var k = 10_000d;
            return RandomInt((int)(min * k), (int)(max * k)) / k;
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
        public static string FormatTime(TimeSpan t)
        {
            return t.Minutes > 1 ? $"{t:m' MINS'}" : t.Minutes > 0 ? $@"{t:m' MIN 's\.fff's'}" : $@"{t:s\.fff's'}";
        }
        
        public  static string SongNameOr(Message m, string  s) => SongNameIn(m) ?? SongNameIn(m.ReplyToMessage) ?? s;
        private static string SongNameIn(Message m) => m?.Audio?.FileName ?? m?.Document?.FileName;
        
        public static bool ChatIsPrivate(long chat) => chat > 0;
        
        public static string GetSenderName(Message m) => m.SenderChat?.Title ?? GetUserFullName(m);
        public static string GetChatTitle (Message m) => (ChatIsPrivate(m.Chat.Id) ? GetUserFullName(m) : m.Chat.Title).Truncate(32);

        public static string GetUserFullName(Message m)
        {
            string name = m.From?.FirstName;
            string last = m.From?.LastName ?? "";
            return last == "" ? name : name + " " + last;
        }

        public static bool IsAprilFools()
        {
            var date = DateTime.Today;
            return date.Month is 4 && date.Day is >= 1 and <= 3;
        }

        public static bool Any() => Random.Next(2) == 0;

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
        
        public static bool HappenedWithinLast(this DateTime date, TimeSpan span)
        {
            return DateTime.Now - date < span;
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

        public static string SET_FREQUENCY_RESPONSE(int n)
        {
            var a = XDDD(Texts.SET_FREQUENCY_RESPONSE);
            var oe = ED(n, "ое", "ые", "ые");
            var  e = ED(n,  "е",  "я",  "й");
            return $"{a} кажд{oe} {(n == 1 ? "ваше" : n)} сообщени{e}";
        }

        public static string HOURS_ED(int hours) => ED(hours, "", "а", "ов");
        public static string  MINS_ED(int  mins) => ED(mins, "у", "ы", "");

        private static string ED(int x, string one, string twoFour, string any)
        {
            if (x % 10 > 4 || x % 10 == 0 || x is > 10 and < 15) return any;
            else if (x % 10 > 1)                                 return twoFour;
            else                                                 return one;
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

        public static bool CheckMatch(ref string dummy, Regex regex)
        {
            var match = regex.Match(dummy);
            if (match.Success) CutCaptureOut(match.Groups[1], ref dummy);

            return match.Success;
        }

        public static void CutCaptureOut(Capture group, ref string text)
        {
            text = text.Remove(group.Index) + "_" + text.Substring(group.Index + group.Length);
        }

        public static string XDDD(string s) => $"{Pick(RANDOM_EMOJI)} {s}";
        public static T Pick<T>(T[] options) => options[Random.Next(options.Length)];

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
            "жди 😎", "загрузка пошла 😮", "✋ ща всё будет", "принял👌", "ваш заказ принят 🥸", "еду скачивать музон 🛒"
        };
        public static readonly string[] PROCESSING_RESPONSE =
        {
            "идёт обработка...", "вжжжжж...", "брррррр..."
        };

        public static readonly string[] RANDOM_EMOJI =
        {
            "🔥✍️", "🪵", "😈", "😎", "💯", "📦", "⚙", "🪤", "💡", "🧨", "🫗", "🌭", "☝️",
            "🍒", "🧄", "🍿", "😭", "🪶", "✨", "🍻", "👌", "💀", "🎳", "🗿", "🔧", "🎉", "🎻"
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
        
        public static string[]   GetFiles      (string path, string pattern = "*")
        {
            Directory.CreateDirectory(path);
            return Directory.GetFiles(path, pattern);
        }

        public static void ClearTempFiles()
        {
            ClearDirectory(TEMP_FOLDER, new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 3 });
            ClearDirectory(FUSE_HISTORY_FOLDER, new EnumerationOptions() { RecurseSubdirectories = false});
        }

        private static void ClearDirectory(string path, EnumerationOptions options)
        {
            if (!Directory.Exists(path)) return;
            try
            {
                var files = Directory.GetFiles(path, "*", options);

                if (options.RecurseSubdirectories) Directory.Delete(path, true);
                else
                {
                    foreach (var file in files) File.Delete(file);
                }

                Log($"DEL TEMP [{path}] >> {files.Length} FILES!", ConsoleColor.Yellow);
            }
            catch (Exception e)
            {
                LogError($"CAN'T DEL TEMP [{path}] >> {e.Message}");
            }
        }
    }
}