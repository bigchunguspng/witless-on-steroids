using Witlesss.Also;

namespace Witlesss.Commands
{
    public class GenerateByFirstWord : WitlessCommand
    {
        public override void Run()
        {
            if (Text.Contains(' '))
            {
                string word = Text.Split()[^1];
                LetterCaseMode mode;
                if      (word == word.ToLower())
                    mode = LetterCaseMode.Lower;
                else if (word == word.ToUpper())
                    mode = LetterCaseMode.Upper;
                else
                    mode = LetterCaseMode.Sentence;

                Text = Text.Substring(Text.IndexOf(' ') + 1);
                Text = Text.Remove(Text.Length - word.Length) + Baka.TryToGenerateFromWord(word.ToLower());
                Bot.SendMessage(Chat, Extension.TextInLetterCase(Text, mode));
                Logger.Log($"{Title} >> FUNNY BY WORD");
            }
            else
                Bot.SendMessage(Chat, Strings.A_MANUAL);
        }
    }
}