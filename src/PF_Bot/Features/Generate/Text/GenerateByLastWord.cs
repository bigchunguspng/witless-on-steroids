using PF_Tools.Backrooms.Helpers;

namespace PF_Bot.Features.Generate.Text
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
                var lines = Args!.Split('\n');
                var words = lines[0].Split();
                
                word = words.Length == 1 ? words[0] : string.Join(' ', words[..2]);
                word = word.ToLower();

                ending = Args[word.Length..];
            }

            var up = Command!.Contains("up");
            var repeats = _repeat.ExtractGroup(1, Command!, int.Parse, 1);
            var texts = new string[repeats];
            for (var i = 0; i < repeats; i++)
            {
                var mode = up ? LetterCase.Upper : GetMode(Args);
                texts[i] = byWord
                    ? Baka.GenerateByLast(word).InLetterCase(mode) + ending
                    : Baka.GenerateBackwards().InLetterCase(mode);
            }

            await Task.Run(() =>
            {
                foreach (var text in texts) Bot.SendMessage(Origin, text, preview: true);
            });

            LogXD(Title, repeats, "FUNNY BY LAST WORD");
        }
    }
}