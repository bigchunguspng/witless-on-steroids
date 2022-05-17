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
                Text = Text.Substring(Text.IndexOf(' ') + 1);
                Text = Text.Remove(Text.Length - word.Length) + Baka.TryToGenerateFromWord(word.ToLower());
                Bot.SendMessage(Chat, Extension.TextInRandomLetterCase(Text));
                Logger.Log($"{Title} >> FUNNY BY WORD");
            }
            else
                Bot.SendMessage(Chat, Strings.A_MANUAL);
        }
    }
}