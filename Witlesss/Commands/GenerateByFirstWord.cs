namespace Witlesss.Commands
{
    public class GenerateByFirstWord : WitlessCommand
    {
        public override void Run()
        {
            if (Text.Contains(' ')) // todo repeater
            {
                var words = Text.Split();
                var word = words[^1];
                if (words.Length > 2)
                {
                    word = string.Join(' ', words[^2..]); // take two last words
                }

                var text = RemoveCommand(words[0]);
                text = text.Remove(text.Length - word.Length) + Baka.GenerateByWord(word.ToLower());
                Bot.SendMessage(Chat, TextInLetterCase(text, GetMode(words[1])));
                Log($"{Title} >> FUNNY BY WORD");
            }
            else
                Bot.SendMessage(Chat, A_MANUAL);
        }

        protected string  RemoveCommand  (string s) => Text.Substring(s.Length + 1);

        protected LetterCaseMode GetMode (string s)
        {
            if (s == s.ToLower()) return LetterCaseMode.Lower;
            if (s == s.ToUpper()) return LetterCaseMode.Upper;
            return                       LetterCaseMode.Sentence;
        }
    }
}