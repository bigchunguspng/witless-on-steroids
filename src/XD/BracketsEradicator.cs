using System.Text.RegularExpressions;

namespace Witlesss.XD
{
    public static class BracketsEradicator
    {
        private static readonly Regex _brackets = new(@"(\s?\(+([^\(]+?)\)+)|(\s?\[+([^\[]+?)\]+)");
        private static readonly Regex _spaces   = new(@"\s+");
        private static readonly MatchEvaluator _evaluator = ReplaceBrackets;

        /// <summary>
        /// Gets rid of () and [] brackets and text inside them.
        /// </summary>
        public static string RemoveBrackets(this string text)
        {
            while (_brackets.IsMatch(text))
            {
                text = _brackets.Replace(text, _evaluator);
            }

            return _spaces.Replace(text, " ").Trim();
        }

        private static string ReplaceBrackets(Match match)
        {
            return match.Value.StartsWith(' ') ? "" : " ";
        }
    }
}