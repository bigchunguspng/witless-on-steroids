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
            var repeats = _repeat.ExtractGroup(1, Command!, int.Parse, 1);
            for (var i = 0; i < repeats; i++)
            {
                var mode = up ? LetterCase.Upper : GetMode(Args);
                var message = byWord ? opening + Baka.GenerateByWord(word) : Baka.Generate();
                Bot.SendMessage(Chat, message.InLetterCase(mode), preview: true);
            }

            LogXD(Title, repeats, "FUNNY BY WORD");
        }

        protected static LetterCase GetMode(string? s)
        {
            if (s is null) return GetRandomLetterCase();
            if (s == s.ToLower()) return LetterCase.Lower;
            if (s == s.ToUpper()) return LetterCase.Upper;
            return IsOneIn(8) ? LetterCase.Upper : LetterCase.Sentence;
        }

        protected static void LogXD(string title, int repeats, string s)
        {
            var message = $"{title} >> {s}";
            if (repeats > 1) message += $" [{repeats}]";
            Log(message);
        }
    }
}