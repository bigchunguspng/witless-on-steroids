namespace Witlesss.Commands.Generation
{
    public class GenerateByLastWord : GenerateByFirstWord
    {
        private static readonly Regex _repeat = new(@"^\/zz\S*([2-9])\S*");

        protected override async Task Run()
        {
            string word = null!, ending = null!;
            var byWord = Args != null;
            if (byWord)
            {
                var words = Args!.Split();
                word = words[0];
                if (words.Length > 1)
                {
                    word = string.Join(' ', words[..2]); // take first two words
                }

                word = word.ToLower();

                ending = Args[word.Length..];
            }

            var up = Command!.Contains("up");
            var repeats = _repeat.ExtractGroup(1, Command!, int.Parse, 1);
            var texts = new string[repeats];
            for (var i = 0; i < repeats; i++)
            {
                var text = byWord ? Baka.GenerateByLast(word.ToLower()) + ending : Baka.GenerateBackwards();
                texts[i] = text.InLetterCase(up ? LetterCase.Upper : GetMode(Args));
            }

            await Task.Run(() =>
            {
                foreach (var text in texts) Bot.SendMessage(Chat, text, preview: true);
            });

            LogXD(Title, repeats, "FUNNY BY LAST WORD");
        }
    }
}