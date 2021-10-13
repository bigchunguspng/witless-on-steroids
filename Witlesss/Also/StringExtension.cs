using static Witlesss.Also.LetterCaseMode;

namespace Witlesss.Also
{
    public static class StringExtension
    {
        public static string InLetterCase(this string text, LetterCase letterCase)
        {
            switch (letterCase.Case)
            {
                case Lower:
                    return text.ToLower();
                case Upper:
                    return text.ToUpper();
                case Sentence:
                    return text[0].ToString().ToUpper() + text.Substring(1).ToLower();
                default:
                    return text;
            }
        }
    }
}