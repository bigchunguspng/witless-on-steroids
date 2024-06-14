using System.Text.RegularExpressions;

namespace Witlesss.Commands
{
    public class GenerateByFirstWord : WitlessSyncCommand
    {
        private static readonly Regex _repeat = new(@"^\/a\S*([2-9])\S*");

        protected override void Run()
        {
            if (Args is null)
            {
                Bot.SendMessage(Chat, A_MANUAL);
            }
            else
            {
                var words = Args.Split();
                var word = words[^1];
                var mode = GetMode(words[0]);
                if (words.Length > 1)
                {
                    word = string.Join(' ', words[^2..]); // take last two words
                }

                word = word.ToLower();

                var text = Text!;
                var outset = text.Remove(text.Length - word.Length);
                var repeats = GetRepeats(_repeat.Match(Command!));
                for (int i = 0; i < repeats; i++)
                {
                    text = outset + Baka.GenerateByWord(word);
                    Bot.SendMessage(Chat, text.ToLetterCase(mode));
                }

                LogXD(Title, repeats, "FUNNY BY WORD");
            }
        }

        protected static LetterCaseMode GetMode (string s)
        {
            if (s == s.ToLower()) return LetterCaseMode.Lower;
            if (s == s.ToUpper()) return LetterCaseMode.Upper;
            return                       LetterCaseMode.Sentence;
        }

        protected static void LogXD(string title, int repeats, string s)
        {
            var message = $"{title} >> {s}";
            if (repeats > 1) message += $" [{repeats}]";
            Log(message);
        }

        protected static int GetRepeats(Match match)
        {
            return match.Success && int.TryParse(match.Groups[1].Value, out var x) ? x : 1;
        }
    }
}