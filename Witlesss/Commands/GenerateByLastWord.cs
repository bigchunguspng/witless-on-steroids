namespace Witlesss.Commands
{
    public class GenerateByLastWord : GenerateByFirstWord
    {
        public override void Run()
        {
            if (Text.Contains(' '))
            {
                var words = Text.Split();
                var word = words[1];
                if (words.Length > 2)
                {
                    word = string.Join(' ', words[1..3]); // take two first words
                }

                var text = RemoveCommand(words[0]);
                text = Baka.GenerateByLastWord(word.ToLower()) + text.Substring(word.Length);
                Bot.SendMessage(Chat, TextInLetterCase(text, GetMode(word)));
                Log($"{Title} >> FUNNY BY LAST WORD");
            }
            else
                Bot.SendMessage(Chat, ZZ_MANUAL);
        }
    }
}