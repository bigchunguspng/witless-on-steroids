using System.Text.RegularExpressions;
using static Witlesss.Commands.MakeMemeCore_Static;

namespace Witlesss.Commands
{
    public class GenerateByLastWord : GenerateByFirstWord
    {
        private static bool REPEAT_RX() => Regex.IsMatch(Text, @"^\/zz\S*\d+\S*");

        public override void Run()
        {
            if (Text.Contains(' '))
            {
                var words = Text.Split();
                var word = words[1];
                var mode = GetMode(word);
                if (words.Length > 2)
                {
                    word = string.Join(' ', words[1..3]); // take first two words
                }
                
                word = word.ToLower();

                var text = RemoveCommand(words[0]);
                var ending = text[word.Length..];
                var repeats = GetRepeats(REPEAT_RX());
                for (int i = 0; i < repeats; i++)
                {
                    text = Baka.GenerateByLast(word.ToLower()) + ending;
                    Bot.SendMessage(Chat, text.ToLetterCase(mode));
                }

                LogXD(repeats, "FUNNY BY LAST WORD");
            }
            else
                Bot.SendMessage(Chat, ZZ_MANUAL);
        }
    }
}