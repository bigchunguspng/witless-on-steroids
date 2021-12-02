using System;
using System.IO;
using System.Linq;
using static System.Environment;
using static Witlesss.Also.LetterCaseMode;
using static Witlesss.Also.Strings;
using static Witlesss.Logger;

namespace Witlesss.Also
{
    public static class Extension
    {
        private static readonly Random Random = new Random();
        
        public static string TextInRandomLetterCase(string text)
        {
            LetterCaseMode c = RandomLetterCase();
            switch (c)
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
            string[] words = text.Split();
            return words.Length > 1 && int.TryParse(words[1], out value);
        }
        
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
                _ => ""
            };
        }

        public static string ValidFileName(string text)
        {
            char[] chars = Path.GetInvalidFileNameChars();
            foreach (char c in chars) text = text.Replace(c, '_');
            return text;
        }

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
            var b = $"\n\n{SET_FREQUENCY_RESPONSE_B} {100 / interval}%";
            return a + b;
        }

        public static string FILE_TOO_BIG_RESPONSE()
        {
            var a = new[] {"пук-среньк...", "не подниму (много весит)", "тяжёлая штука", "ого, какой большой", "сорян, не влезает", ""};
            return a.ElementAt(Random.Next(a.Length));
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

        public static int AssumedResponseTime(int initialTime, string text)
        {
            if (text == null) return initialTime;
            return Math.Min(text.Length, 120) * 25;
        }

        public static void CreatePath(string path) => Directory.CreateDirectory(path.Remove(path.LastIndexOf('\\')));

        public static void ClearTempFiles()
        {
            int filesDeleted = 0, dirsDeleted = 0, filesMoved = 0;
            var path = $@"{CurrentDirectory}\{PICTURES_FOLDER}";
            if (!Directory.Exists(path))
                return;
            
            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                string[] files = Directory.GetFiles(directory);
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
                            filesMoved++;
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
            Log($"Удалено: {filesDeleted} ненужных файлов и {dirsDeleted} пустых папок! {filesMoved} файлов перемещено.", ConsoleColor.Yellow);
        }
    }
}