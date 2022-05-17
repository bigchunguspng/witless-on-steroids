using System;
using System.Linq;
using Witlesss.Also;

namespace Witlesss.Commands
{
    public class Buhurt : WitlessCommand
    {
        public override void Run()
        {
            var length = 3;
            if (Extension.HasIntArgument(Text, out int value))
                length = Math.Clamp(value, 2, 13);
            var lines = new string[length];
            for (var i = 0; i < length; i++)
                lines[i] = Baka.TryToGenerate().ToUpper();
            string result = string.Join("\n@\n", lines.Distinct());
            Bot.SendMessage(Chat, result);
            Logger.Log($"{Title} >> BUGURT #@#");
        }
    }
}