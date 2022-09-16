using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        public static readonly Random Random = new Random();

        public static int AssumedResponseTime(int initialTime, string text)
        {
            if (text == null) return initialTime;
            return Math.Min(text.Length, 120) * 25;
        }

        public static string TextInRandomLetterCase(string text) => TextInLetterCase(text, RandomLetterCase());
        public static string TextInLetterCase(string text, LetterCaseMode mode)
        {
            switch (mode)
            {
                case Lower:
                    return text.ToLower();
                case Upper:
                    return text.ToUpper();
                case Sentence:
                    return text[0].ToString().ToUpper() + text.Substring(1).ToLower();
                default:
                    return text;
            }
        }

        private static LetterCaseMode RandomLetterCase()
        {
            int n = Random.Next(8);
            if (n < 5)
                return Lower;
            else if (n < 7)
                return Sentence;
            else
                return Upper;
        }

        public static string SenderName(Message message) => message.SenderChat?.Title ?? UserFullName(message);
        public static string TitleOrUsername(Message message) => Truncate(message.Chat.Id < 0 ? message.Chat.Title : UserFullName(message), 32);

        private static string Truncate(string s, int length) => s.Length > length ? s.Substring(0, length - 3) + "..." : s;
        private static string UserFullName(Message message)
        {
            string name = message.From?.FirstName;
            string last = message.From?.LastName ?? "";
            return last == "" ? name : name + " " + last;
        }

        public static void GetDemotivatorText(Witless witless, string text, out string a, out string b)
        {
            b = witless.TryToGenerate();
            if (b.Length > 1) b = b[0] + b.Substring(1).ToLower(); // lower text can't be UPPERCASE
            if (text != null && text.Contains(' ')) // custom upper text
            {
                a = text.Substring(text.IndexOf(' ') + 1);
                if (a.Contains('\n')) // custom bottom text
                {
                    b = a.Substring(a.IndexOf('\n') + 1);
                    a = a.Remove(a.IndexOf('\n'));
                }
            }
            else
                a = witless.TryToGenerate();
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

        public static bool StringIsTimeSpan(string arg, out TimeSpan span)
        {
            span = TimeSpan.Zero;
            arg = arg.TrimStart('-').Replace('.', ',').Replace('ю', ',').Replace('б', ',');
            if (double.TryParse(arg, out double seconds))
            {
                span = TimeSpan.FromSeconds(seconds);
                return true;
            }
            arg = arg.Replace('^', ':').Replace(';', ':').Replace('Ж', ':');
            if (arg.Contains(':'))
            {
                if (TimeSpan.TryParseExact(arg, "m\\:ss", null, out span)) return true;
            }

            return false;
        }
        
        public static string FormatDouble(double d) => d.ToString(CultureInfo.InvariantCulture);

        public static string UniquePath(string path, string extension = "")
        {
            while (File.Exists(path) || Directory.Exists(path))
            {
                int nameStartIndex = path.LastIndexOf('\\') + 1;
                string name = path.Substring(nameStartIndex);
                string directory = path.Remove(nameStartIndex);
                
                if (extension != "")
                    name = name.Replace(extension, "");
                int underscoreIndex = name.LastIndexOf('_');
                if (underscoreIndex > 0 && int.TryParse(name.Substring(underscoreIndex + 1), out int n))
                {
                    int number = n + 1;
                    name = name.Remove(underscoreIndex + 1) + number;
                }
                else
                    name = name + "_0";

                path = directory + name + extension;
            }
            return path;
        }
        public static string ValidFileName(string text)
        {
            var chars = Path.GetInvalidFileNameChars();
            foreach (char c in chars) text = text.Replace(c, '_');
            return text;
        }

        public static void SetOutName(string path, out string output, string suffix)
        {
            string extension = GetFileExtension(path);
            output = GetFileName(path) + suffix + extension;
        }
        public static void SetOutName(string path, out string output, string suffix, out bool video)
        {
            string extension = GetFileExtension(path);
            output = GetFileName(path) + suffix + extension;
            video = extension == ".mp4";
        }

        public static string GetFileName(string path) => path.Remove(path.LastIndexOf('.'));
        public static string GetFileExtension(string path) => path.Substring(path.LastIndexOf('.'));
        public static string ShortID(string fileID) => fileID.Remove(62).Remove(2, 44);

        public static string ExtensionFromID(string id)
        {
            string id2 = id.Remove(2);
            return id2 switch
            {
                "BA" => ".mp4",
                "Cg" => ".mp4", // animation
                "CQ" => ".mp3",
                "Aw" => ".mp3", // voice message / ogg
                "BQ" => ".wav",
                "Ag" => ".jpg",
                "CA" => ".webm",
                _    => ""
            };
        }
        public static MediaType MediaTypeFromID(string id)
        {
            string id2 = id.Remove(2);
            return id2 switch
            {
                "BA" => MediaType.AudioVideo,
                "Cg" => MediaType.Video,
                "CQ" => MediaType.Audio,
                "Aw" => MediaType.Audio,
                "BQ" => MediaType.Audio,
                "CA" => MediaType.Video,
                _    => MediaType.Audio // bc who cares?
            };
        }

        public static IList<string> RemoveEmpties(IList<string> list) => list.Where(s => !string.IsNullOrEmpty(s)).ToList();

        public static string SET_FREQUENCY_RESPONSE(int interval)
        {
            string a = SET_FREQUENCY_RESPONSE_A;
            if (interval % 10 > 4 || interval % 10 == 0 || interval > 10 && interval < 15)
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

        public static string FILE_TOO_BIG_RESPONSE()
        {
            var a = new[] {"пук-среньк...", "много весит 🥺", "тяжёлая штука 🤔", "ого, какой большой 😯", "какой тяжёлый 😩"};
            return a[Random.Next(a.Length)];
        }

        public static string FileSize(string path)
        {
            long bytes = new FileInfo(path).Length;
            long kbs = bytes / 1024;
            if (kbs < 1)
                return bytes + " байт";
            else
                return kbs + " КБ";
        }

        public static bool FileEmptyOrNotExist(string path) => !File.Exists(path) || new FileInfo(path).Length == 0;

        public static void CreatePath(string path) => Directory.CreateDirectory(path.Remove(path.LastIndexOf('\\')));

        public static void ClearTempFiles()
        {
            int filesDeleted = 0, dirsDeleted = 0;
            var path = $@"{CurrentDirectory}\{PICTURES_FOLDER}";
            if (!Directory.Exists(path))
                return;
            
            var directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                var files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    if (file.EndsWith(".jpg"))
                    {
                        try
                        {
                            File.Delete(file);
                            filesDeleted++;
                        }
                        catch
                        {
                            //
                        }
                    }
                    else
                    {
                        try
                        {
                            string extension = Path.GetExtension(file);
                            string destination = file.Replace($@"{file.Split('\\')[^2]}\", "");
                            File.Move(file, UniquePath(destination, extension));
                        }
                        catch
                        {
                            //
                        }
                    }
                }
                try
                {
                    Directory.Delete(directory);
                    dirsDeleted++;
                }
                catch
                {
                    //
                }
            }
            try
            {
                Directory.Delete($@"{CurrentDirectory}\{DEBUG_FOLDER}", true);
                dirsDeleted++;
            }
            catch
            {
                //
            }
            Log($"DEL TEMP >> {filesDeleted} FILES + {dirsDeleted} DIRS!", ConsoleColor.Yellow);
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