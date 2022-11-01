using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using static System.Environment;
using static Witlesss.LetterCaseMode;
using static Witlesss.Strings;
using static Witlesss.Logger;
using File = System.IO.File;

namespace Witlesss
{
    public static class Extension
    {
        private static readonly Regex Column = new("[:;^Жж]"), Comma = new("[.юб]");
        
        public static readonly Random Random = new();

        public static int AssumedResponseTime(int initialTime, string text)
        {
            if (text == null) return initialTime;
            return Math.Min(text.Length, 120) * 25;
        }

        public static string TextInRandomLetterCase(string text) => TextInLetterCase(text, RandomLetterCase());
        public static string TextInLetterCase(string text, LetterCaseMode mode) => mode switch
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

        public static string SenderName(Message message) => message.SenderChat?.Title ?? UserFullName(message);
        public static string TitleOrUsername(Message message) => Truncate(message.Chat.Id < 0 ? message.Chat.Title : UserFullName(message), 32);

        private static string Truncate(string s, int length) => s.Length > length ? s[..(length - 3)] + "..." : s;
        private static string UserFullName(Message message)
        {
            string name = message.From?.FirstName;
            string last = message.From?.LastName ?? "";
            return last == "" ? name : name + " " + last;
        }

        public static void GetDemotivatorText(Witless witless, string text, out string a, out string b)
        {
            b = witless.TryToGenerate();
            if (b.Length > 1) b = b[0] + b[1..].ToLower(); // lower text can't be UPPERCASE
            if (string.IsNullOrEmpty(text)) a = witless.TryToGenerate();
            else
            {
                var s = text.Split('\n', 2);
                a = s[0];
                if (s.Length > 1) b = s[1];
            }
        }

        public static bool HasIntArgument(string text, out int value)
        {
            value = 0;
            var words = text.Split();
            return words.Length > 1 && int.TryParse(words[1], out value);
        }
        public static bool HasDoubleArgument(string text, out double value)
        {
            value = 0;
            var words = text.Split();
            return words.Length > 1 && double.TryParse(words[1].Replace('.', ','), out value);
        }

        public static bool TextIsTimeSpan(string arg, out TimeSpan span)
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

        public static string UniquePath(string path, string extension = "", bool extra = false)
        {
            while (File.Exists(path) || Directory.Exists(path) || extra)
            {
                int nameStartIndex = path!.LastIndexOf('\\') + 1;
                string name = path[nameStartIndex..];
                string directory = path.Remove(nameStartIndex);
                
                if (extension != "")
                    name = name.Replace(extension, "");
                int underscoreIndex = name.LastIndexOf('_');
                if (underscoreIndex > 0 && int.TryParse(name.AsSpan(underscoreIndex + 1), out int n))
                {
                    int number = n + 1;
                    name = name.Remove(underscoreIndex + 1) + number;
                }
                else
                    name += "_0";

                path = directory + name + extension;
                extra = false;
            }
            return path;
        }
        public static string ValidFileName(string text)
        {
            var chars = Path.GetInvalidFileNameChars();
            return chars.Aggregate(text, (current, c) => current.Replace(c, '_'));
        }

        public static string SetOutName(string path, string suffix)
        {
            string extension = Path.GetExtension(path);
            return RemoveExtension(path) + suffix + extension;
        }
        public static string SetOutName(string path, string suffix, out bool video)
        {
            string extension = Path.GetExtension(path);
            video = extension == ".mp4";
            return RemoveExtension(path) + suffix + extension;
        }

        private static string RemoveExtension(string path) => path.Remove(path.LastIndexOf('.'));

        public static string ShortID(string fileID) => fileID.Remove(62).Remove(2, 44);
        public static string ExtensionFromID(string id) => ExtensionsIDs[id.Remove(2)];
        public static MediaType MediaTypeFromID(string id) => MediaTypes[id.Remove(2)];

        private static readonly Dictionary<string, string> ExtensionsIDs = new()
        {
            {"BA", ".mp4"}, {"Cg", ".mp4"}, // Cg - animation
            {"CQ", ".mp3"}, {"Aw", ".mp3"}, // Aw - voice message / ogg
            {"BQ", ".wav"}, {"Ag", ".jpg"}, {"CA", ".webm"}
        };
        private static readonly Dictionary<string, MediaType> MediaTypes = new()
        {
            {"BA", MediaType.AudioVideo}, {"Cg", MediaType.Video}, {"CA", MediaType.Video},
            {"Aw", MediaType.Audio},      {"BQ", MediaType.Audio}, {"CQ", MediaType.Audio}
        };

        public static IList<string> RemoveEmpties(IList<string> list) => list.Where(s => !string.IsNullOrEmpty(s)).ToList();

        public static string SET_FREQUENCY_RESPONSE(int interval)
        {
            string a = SET_FREQUENCY_RESPONSE_A;
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

        public static string SET_PROBABILITY_RESPONSE(int p) => $"{SET_FREQUENCY_RESPONSE_B} {p}%";

        public static string Pick(string[] responses) => responses[Random.Next(responses.Length)];

        public static readonly string[] FILE_TOO_BIG_RESPONSE = new string[]
        {
            "пук-среньк...", "много весит 🥺", "тяжёлая штука 🤔", "ого, какой большой 😯", "какой тяжёлый 😩"
        };
        public static readonly string[] UNKNOWN_CHAT_RESPONSE = new string[]
        {
            "ты кто?", "я тебя не знаю чувак 😤", "сними маску, я тебя не узнаю", "а ты кто 😲", "понасоздают каналов... 😒"
        };
        public static readonly string[] NOT_ADMIN_RESPONSE = new string[]
        {
            "ты не админ 😎", "ты не админ чувак 😒", "попроси админа", "у тебя нет админки 😎", "будет админка - приходи"
        };

        public static string FileSize(string path)
        {
            long bytes = SizeInBytes(path);
            long kbs = bytes / 1024;
            return kbs switch { < 1 => bytes + " байт", _ => kbs + " КБ" };
        }

        public static long SizeInBytes(string path) => new FileInfo(path).Length;

        public static bool FileEmptyOrNotExist(string path) => !File.Exists(path) || SizeInBytes(path) == 0;

        public static void CreateFilePath(string path) => Directory.CreateDirectory(path.Remove(path.LastIndexOf('\\')));

        public static FileInfo[] GetFilesInfo(string path)
        {
            Directory.CreateDirectory(path);
            return new DirectoryInfo(path).GetFiles();
        }
        
        public static string[] GetFiles(string path)
        {
            Directory.CreateDirectory(path);
            return Directory.GetFiles(path);
        }

        public static void ClearTempFiles()
        {
            var temp = $@"{CurrentDirectory}\{TEMP_FOLDER}";
            if (!Directory.Exists(temp)) return;
            try
            {
                var x = Directory.GetFiles(temp).Length;
                Directory.Delete(temp, true);
                Log($"DEL TEMP >> {x} FILES!", ConsoleColor.Yellow);
            }
            catch (Exception e)
            {
                LogError("CAN'T DEL TEMP >> " + e.Message);
            }
        }
    }

    public enum SpeedMode
    {
        Fast,
        Slow
    }

    public enum MediaType
    {
        Audio,
        Video,
        AudioVideo
    }
    
    public enum LetterCaseMode
    {
        Lower,
        Upper,
        Sentence
    }
}