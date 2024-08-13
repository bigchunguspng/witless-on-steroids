using System.Text.RegularExpressions;

namespace Witlesss.Commands.Generation
{
    public class GenerateByFirstWord : WitlessSyncCommand
    {
        private static readonly Regex _repeat = new(@"^\/a\S*([2-9])\S*");

        protected override void Run()
        {
            string word = null!, opening = null!;
            var byWord = Args != null;
            if (byWord)
            {
                var words = Args!.Split();
                word = words[^1];
                if (words.Length > 1)
                {
                    word = string.Join(' ', words[^2..]); // take last two words
                }

                word = word.ToLower();

                opening = Args.Remove(Args.Length - word.Length);
            }

            var up = Command!.Contains("up");
            var repeats = GetRepeats(_repeat.Match(Command!));
            for (var i = 0; i < repeats; i++)
            {
                var mode = up ? LetterCaseMode.Upper : GetMode(Args);
                var message = byWord ? opening + Baka.GenerateByWord(word) : Baka.Generate();
                Bot.SendMessage(Chat, message.InLetterCase(mode), preview: true);
            }

            LogXD(Title, repeats, "FUNNY BY WORD");
        }

        protected static LetterCaseMode GetMode(string? s)
        {
            if (s is null)
                return StringExtensions.GetRandomLetterCase();
            if (s == s.ToLower()) return LetterCaseMode.Lower;
            if (s == s.ToUpper()) return LetterCaseMode.Upper;
            return StringExtensions.GetRandomLetterCase();
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