using System.Text.RegularExpressions;

namespace Witlesss.Commands
{
    public class GenerateByLastWord : GenerateByFirstWord
    {
        private readonly Regex _repeat = new(@"^\/zz\S*([2-9])\S*");

        protected override void Run()
        {
            if (Args is null)
            {
                Bot.SendMessage(Chat, ZZ_MANUAL);
            }
            else
            {
                var words = Args.Split();
                var word = words[0];
                var mode = GetMode(word);
                if (words.Length > 1)
                {
                    word = string.Join(' ', words[..2]); // take first two words
                }

                word = word.ToLower();

                var text = Args!;
                var ending = text[word.Length..];
                var repeats = GetRepeats(_repeat.Match(Command!));
                for (int i = 0; i < repeats; i++)
                {
                    text = Baka.GenerateByLast(word.ToLower()) + ending;
                    Bot.SendMessage(Chat, text.ToLetterCase(mode));
                }

                LogXD(Title, repeats, "FUNNY BY LAST WORD");
            }
        }
    }
}