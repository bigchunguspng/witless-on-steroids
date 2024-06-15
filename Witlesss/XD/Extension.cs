using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Witlesss.XD
{
    public static class Extension
    {
        private static readonly Regex Column = new("[:;^Жж]"), Comma = new("[.юб]");
        public  static readonly Regex PngJpg = new("(.png)|(.jpg)"), EmojiRegex = new (REGEX_EMOJI);
        public  static readonly Regex FFmpeg = new(@"ffmpeg|ffprobe", RegexOptions.IgnoreCase);
        private static readonly Regex Errors = new(@"One or more errors occurred. \((\S*(\s*\S)*)\)");

        public static bool IsOneIn         (int x) => Random.Shared.Next(x) == 0;
        public static bool IsFirstOf(int a, int b) => Random.Shared.Next(a + b) < a;

        public static bool Lucky(int chance, int max = 100) => Random.Shared.Next(max) < chance;

        public static int    RandomInt   (int    min, int    max) => Random.Shared.Next(min, max + 1);
        public static double RandomDouble(double min, double max)
        {
            var k = 10_000d;
            return RandomInt((int)(min * k), (int)(max * k)) / k;
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

        public static string UniquePath(string path, bool extra = false)
        {
            var cd = true;
            var directory = Path.GetDirectoryName(path)!;
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

                path = Path.Combine(directory, $"{name}{extension}");
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

        public static string XDDD(string s) => $"{Pick(RANDOM_EMOJI)} {s}";
        public static T Pick<T>(T[] options) => options[Random.Shared.Next(options.Length)];

        public static readonly string[] FILE_TOO_BIG_RESPONSE =
        [
            "пук-среньк...", "много весит 🥺", "тяжёлая штука 🤔", "ого, какой большой 😯", "какой тяжёлый 😩"
        ];
        public static readonly string[] UNKNOWN_CHAT_RESPONSE =
        [
            "ты кто?", "я тебя не знаю чувак 😤", "сними маску, я тебя не узнаю", "а ты кто 😲", "понасоздают каналов... 😒"
        ];
        public static readonly string[] NOT_ADMIN_RESPONSE =
        [
            "ты не админ 😎", "ты не админ чувак 😒", "попроси админа", "у тебя нет админки 😎", "будет админка - приходи"
        ];
        public static readonly string[] I_FORGOR_RESPONSE =
        [
            "Сорян, не помню", "Сорян, не помню такого", "Забыл уже", "Не помню", "Я бы скинул, но уже потерял её"
        ];
        public static readonly string[] PLS_WAIT_RESPONSE =
        [
            "жди 😎", "загрузка пошла 😮", "✋ ща всё будет", "принял👌", "ваш заказ принят 🥸", "еду скачивать музон 🛒"
        ];
        public static readonly string[] PROCESSING_RESPONSE =
        [
            "идёт обработка...", "вжжжжж...", "брррррр..."
        ];

        public static readonly string[] RANDOM_EMOJI =
        [
            "🔥✍️", "🪵", "😈", "😎", "💯", "📦", "⚙", "🪤", "💡", "🧨", "🫗", "🌭", "☝️",
            "🍒", "🧄", "🍿", "😭", "🪶", "✨", "🍻", "👌", "💀", "🎳", "🗿", "🔧", "🎉", "🎻"
        ];
        public static readonly string[] FAIL_EMOJI_1 = ["🤣", "😎", "🥰", "☺️", "💀", "😤", "😩"];
        public static readonly string[] FAIL_EMOJI_2 = ["😵", "😧", "😨", "😰", "😮", "😲", "💀"];

        public static string GetRandomASCII()
        {
            var files = GetFiles(Paths.Dir_ASCII);
            return File.ReadAllText(files[Random.Shared.Next(files.Length)]);
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

        public static FileInfo[] GetFilesInfo  (string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Directory.CreateDirectory(path);
            return new  DirectoryInfo(path).GetFiles("*", searchOption);
        }
        
        public static string[]   GetFiles      (string path, string pattern = "*")
        {
            Directory.CreateDirectory(path);
            return Directory.GetFiles(path, pattern);
        }

        public static void ClearTempFiles()
        {
            ClearDirectory(Paths.Dir_Temp, new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 3 });
            ClearDirectory(Paths.Dir_History, new EnumerationOptions() { RecurseSubdirectories = false });
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

        public static System.Drawing.Size Ok(this Size size) => new(size.Width, size.Height);
        public static Size Ok(this System.Drawing.Size size) => new(size.Width, size.Height);

        public static T GetRandomMemeber<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(Random.Shared.Next(values.Length))!;
        }
    }

    public static class Helpers
    {
        public static Stopwatch GetStartedStopwatch()
        {
            var sw = new Stopwatch();
            sw.Start();
            return sw;
        }

        public static void Log(this Stopwatch sw, string message)
        {
            Logger.Log($"{sw.Elapsed.TotalSeconds:##0.00000}\t{message}");
            sw.Restart();
        }

        public static T MeasureTime<T>(Func<T> func, string caption)
        {
            var sw = GetStartedStopwatch();
            var result = func.Invoke();
            sw.Log(caption);
            return result;
        }
    }
}