namespace Witlesss.Commands.Generation
{
    public class GenerateByFirstWord : WitlessAsyncCommand
    {
        private static readonly Regex _repeat = new(@"^\/a\S*([2-9])\S*");

        protected override async Task Run()
        {
            string word = null!, opening = null!;
            var byWord = Args != null;
            if (byWord)
            {
                var lines = Args!.Split('\n');
                var words = lines[^1].Split();

                word = words.Length == 1 ? words[^1] : string.Join(' ', words[^2..]);
                word = word.ToLower();

                opening = Args.Remove(Args.Length - word.Length);
            }

            var up = Command!.Contains("up");
            var repeats = _repeat.ExtractGroup(1, Command!, int.Parse, 1);
            var texts = new string[repeats];
            for (var i = 0; i < repeats; i++)
            {
                var text = byWord ? opening + Baka.GenerateByWord(word) : Baka.Generate();
                texts[i] = text.InLetterCase(up ? LetterCase.Upper : GetMode(Args));
            }

            await Task.Run(() =>
            {
                foreach (var text in texts) Bot.SendMessage(Chat, text, preview: true);
            });

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