namespace Witlesss.Commands
{
    public class GenerateByLastWord : GenerateByFirstWord
    {
        public override void Run()
        {
            if (Text.Contains(' '))
            {
                var words = Text.Split();
                string word = words[1];
                if (words.Length > 2)
                {
                    word = string.Join(' ', words[1..3]);
                }

                Text = Text.Substring(words[0].Length + 1);
                Text = Baka.GenerateByLastWord(word.ToLower()) + Text.Substring(word.Length);
                Bot.SendMessage(Chat, TextInLetterCase(Text, GetMode(word)));
                Log($"{Title} >> FUNNY BY LAST WORD");
            }
            else
                Bot.SendMessage(Chat, ZZ_MANUAL);
        }
    }
}