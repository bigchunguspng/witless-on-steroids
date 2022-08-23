using Witlesss.Also;

namespace Witlesss.Commands
{
    public class GenerateByLastWord : GenerateByFirstWord
    {
        public override void Run()
        {
            if (Text.Contains(' '))
            {
                string word = Text.Split()[1];

                Text = Text.Substring(Text.IndexOf(' ') + 1);
                Text = Baka.GenerateByWordBackwards(word.ToLower()) + Text.Substring(word.Length);
                Bot.SendMessage(Chat, Extension.TextInLetterCase(Text, GetMode(word)));
                Logger.Log($"{Title} >> FUNNY BY LAST WORD");
            }
            else
                Bot.SendMessage(Chat, Strings.ZZ_MANUAL);
        }
    }
}