using System.IO;
using static Witlesss.Also.LetterCaseMode;

namespace Witlesss.Also
{
    public static class Extension
    {
        public static string InLetterCase(this string text, LetterCase letterCase)
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
        
        public static void GetDemotivatorText(this Witless witless, string text, out string a, out string b)
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
    }
}