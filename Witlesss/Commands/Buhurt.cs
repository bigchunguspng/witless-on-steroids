using System;
using System.Linq;

namespace Witlesss.Commands
{
    public class Buhurt : WitlessCommand
    {
        public override void Run()
        {
            var length = 3;
            if (HasIntArgument(Text, out int value))
                length = Math.Clamp(value, 2, 15);
            var lines = new string[length];
            for (var i = 0; i < length; i++)
                lines[i] = Baka.TryToGenerate().ToUpper();
            string result = string.Join("\n@\n", lines.Where(x => !string.IsNullOrEmpty(x))).Replace(" @ ", "\n@\n");
            Bot.SendMessage(Chat, result);
            Log($"{Title} >> BUGURT #@#");
        }
    }
}