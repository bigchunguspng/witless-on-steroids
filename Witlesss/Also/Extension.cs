using System;
using System.IO;
using static System.Environment;
using static Witlesss.Also.LetterCaseMode;
using static Witlesss.Also.Strings;
using static Witlesss.Logger;

namespace Witlesss.Also
{
    public static class Extension
    {
        public static string TextInLetterCase(string text, LetterCase letterCase)
        {
            switch (letterCase.Case)
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
        
        public static void GetDemotivatorText(Witless witless, string text, out string a, out string b)
        {
            b = witless.TryToGenerate();
            b = b[0] + b.Substring(1).ToLower(); // lower text can't be UPPERCASE
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
        
        public static string SET_FREQUENCY_RESPONSE(int interval)
        {
            string a = SET_FREQUENCY_RESPONSE_A;
            if (interval % 10 > 4 || interval % 10 == 0 || interval > 10 && interval < 15)
                a = $"{a} каждые {interval} сообщений";
            else if (interval % 10 > 1)
                a = $"{a} каждые {interval} сообщения";
            else if (interval == 1)
                a = $"{a} после каждого вашего сообщения";
            else
                a = $"{a} раз в {interval} сообщение";
            var b = $"\n\n{SET_FREQUENCY_RESPONSE_B} {100 / interval}%";
            return a + b;
        }
        
        public static void ClearExtractedFrames()
        {
            int deleted = 0, failed = 0;
            string path = $@"{CurrentDirectory}\{PICTURES_FOLDER}";
            var directories = Directory.GetDirectories(path);
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
                            deleted++;
                        }
                        catch
                        {
                            failed++;
                        }
                    }
                    else
                    {
                        try
                        {
                            File.Move(file, file.Replace($@"{file.Split('\\')[^2]}\",""));
                        }
                        catch
                        {
                            failed++;
                        }
                    }
                }
                try
                {
                    Directory.Delete(directory);
                }
                catch
                {
                    failed++;
                }
            }
            Log($"Удалено {deleted} ненужных файлов!{(failed > 0 ? $" {failed} элементов осталось" : "")}", ConsoleColor.Yellow);
        }
    }
}