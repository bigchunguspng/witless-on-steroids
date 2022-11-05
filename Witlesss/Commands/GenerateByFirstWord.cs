namespace Witlesss.Commands
{
    public class GenerateByFirstWord : WitlessCommand
    {
        public override void Run()
        {
            if (Text.Contains(' ')) // todo fix one word output
            {
                var words = Text.Split();
                string word = words[^1];
                string start = words[1];
                if (words.Length > 2)
                {
                    word = string.Join(' ', words[^2..]);
                }

                Text = Text.Substring(words[0].Length + 1);
                Text = Text.Remove(Text.Length - word.Length) + Baka.GenerateByWord(word.ToLower());
                Bot.SendMessage(Chat, TextInLetterCase(Text, GetMode(start)));
                Log($"{Title} >> FUNNY BY WORD");
            }
            else
                Bot.SendMessage(Chat, A_MANUAL);
        }

        protected LetterCaseMode GetMode(string s)
        {
            if      (s == s.ToLower())
                return LetterCaseMode.Lower;
            else if (s == s.ToUpper() && s.Length > 1)
                return LetterCaseMode.Upper;
            else
                return LetterCaseMode.Sentence;
        }
    }
}