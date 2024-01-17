using System.Text.RegularExpressions;

namespace Witlesss.Commands
{
    public class GenerateByFirstWord : WitlessCommand
    {
        private static readonly Regex _repeat = new(@"^\/a\S*([2-9])\S*");

        public override void Run()
        {
            if (Text.Contains(' '))
            {
                var words = Text.Split();
                var word = words[^1];
                var mode = GetMode(words[1]);
                if (words.Length > 2)
                {
                    word = string.Join(' ', words[^2..]); // take last two words
                }

                word = word.ToLower();

                var text = RemoveCommand(words[0]);
                var outset = text.Remove(text.Length - word.Length);
                var repeats = GetRepeats(_repeat.Match(Text));
                for (int i = 0; i < repeats; i++)
                {
                    text = outset + Baka.GenerateByWord(word);
                    Bot.SendMessage(Chat, text.ToLetterCase(mode));
                }

                LogXD(repeats, "FUNNY BY WORD");
            }
            else
                Bot.SendMessage(Chat, A_MANUAL);
        }

        protected static LetterCaseMode GetMode (string s)
        {
            if (s == s.ToLower()) return LetterCaseMode.Lower;
            if (s == s.ToUpper()) return LetterCaseMode.Upper;
            return                       LetterCaseMode.Sentence;
        }

        protected static string RemoveCommand   (string s) => Text[(s.Length + 1)..];

        protected static void LogXD(int repeats, string s)
        {
            var message = $"{Title} >> {s}";
            if (repeats > 1) message += $" [{repeats}]";
            Log(message);
        }

        protected static int GetRepeats(Match match)
        {
            var repeats = 1;
            if (match.Success && int.TryParse(match.Groups[1].Value, out var x))
            {
                repeats = x;
            }
            return repeats;
        }
    }
}