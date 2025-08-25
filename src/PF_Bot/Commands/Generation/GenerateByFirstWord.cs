namespace PF_Bot.Commands.Generation
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
                var mode = up ? LetterCase.Upper : GetMode(Args);
                texts[i] = byWord
                    ? opening + Baka.GenerateByWord(word).InLetterCase(mode)
                    : Baka.Generate().InLetterCase(mode);
            }

            await Task.Run(() =>
            {
                foreach (var text in texts) Bot.SendMessage(Origin, text, preview: true);
            });

            LogXD(Title, repeats, "FUNNY BY WORD");
        }

        private static readonly Regex _upper = new("[A-ZА-Я]"), _lower = new("[a-zа-я]");

        protected static LetterCase GetMode(string? s)
        {
            if (s is null) return GetRandomLetterCase();
            var u = _upper.Count(s);
            var l = _lower.Count(s);
            var n = s.Contains("\n\n");
            return n
                ? GetUpperOrLowerLetterCase()
                : u > l
                    ? LetterCase.Upper
                    : LetterCase.Lower;
        }

        protected static void LogXD(string title, int repeats, string s)
        {
            var message = $"{title} >> {s}";
            if (repeats > 1) message += $" [{repeats}]";
            Log(message);
        }
    }
}