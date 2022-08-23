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
                string start = Text.Split()[1];

                Text = Text.Substring(Text.IndexOf(' ') + 1);
                Text = Text.Remove(Text.Length - word.Length) + Baka.GenerateByWord(word.ToLower());
                Bot.SendMessage(Chat, Extension.TextInLetterCase(Text, GetMode(start)));
                Logger.Log($"{Title} >> FUNNY BY WORD");
            }
            else
                Bot.SendMessage(Chat, Strings.A_MANUAL);
        }

        protected LetterCaseMode GetMode(string word)
        {
            if      (word == word.ToLower())
                return LetterCaseMode.Lower;
            else if (word == word.ToUpper() && word.Length > 1)
                return LetterCaseMode.Upper;
            else
                return LetterCaseMode.Sentence;
        }
    }
}