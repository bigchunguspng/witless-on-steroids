using System;
using static Witlesss.Also.LetterCaseMode;

namespace Witlesss.Also
{
    public class LetterCase
    {
        private readonly Random _random;
        private LetterCaseMode _letterCase;
        
        public LetterCase() => _random = new Random();

        public LetterCaseMode Case
        {
            get
            {
                Randomize();
                return _letterCase;
            }
        }

        private void Randomize()
        {
            int n = _random.Next(8);
            if (n < 5)
                _letterCase = Lower;
            else if (n < 7)
                _letterCase = Sentence;
            else
                _letterCase = Upper;
        }
    }
}