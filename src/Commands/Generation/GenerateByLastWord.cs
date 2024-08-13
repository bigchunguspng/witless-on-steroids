using System.Text.RegularExpressions;

namespace Witlesss.Commands.Generation
{
    public class GenerateByLastWord : GenerateByFirstWord
    {
        private static readonly Regex _repeat = new(@"^\/zz\S*([2-9])\S*");

        protected override void Run()
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
            var repeats = GetRepeats(_repeat.Match(Command!));
            for (var i = 0; i < repeats; i++)
            {
                var mode = up ? LetterCaseMode.Upper : GetMode(Args);
                var message = byWord ? Baka.GenerateByLast(word.ToLower()) + ending : Baka.GenerateBackwards();
                Bot.SendMessage(Chat, message.InLetterCase(mode), preview: true);
            }

            LogXD(Title, repeats, "FUNNY BY LAST WORD");
        }
    }
}