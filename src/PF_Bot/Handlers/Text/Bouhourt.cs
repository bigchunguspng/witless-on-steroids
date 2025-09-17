using System.Text;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Handlers.Text
{
    public class Bouhourt : WitlessSyncCommand
    {
        private readonly Regex _rgx_length = new(@"\d+");

        protected override void Run()
        {
            var length = _rgx_length.ExtractGroup(0, Command!, int.Parse, 3);
            var start = Args;

            var greentext = Command!.Contains("bb");
            var sign = greentext ? '>' : '@';

            var lines = new List<string>(length) { start ?? GenerateLine() };
            for (var i = 1; i < length; i++) lines.Add(GenerateLine());

            var sb = new StringBuilder();
            if (greentext)
            {
                foreach (var line in lines) sb.Append('>').AppendLine(line);

                if (Fortune.IsOneIn(3))
                {
                    if (Fortune.IsOneIn(2)) sb.Append('\n').Append(GenerateLine());
                    else sb.Append(GenerateLine().InLetterCase(LetterCase.Sentence));
                }
            }
            else
            {
                for (var i = 0; i < lines.Count; i++)
                {
                    sb.Append(lines[i].ToUpper());
                    if (i + 1 < lines.Count) sb.Append("\n@\n");
                }
            }

            Bot.SendMessage(Origin, sb.ToString(), preview: true);
            Log($"{Title} >> {(greentext ? ">GREENTEXT >" : "BUGURT #@#")}{length}");

            string GenerateLine() => Baka.Generate().Split(sign, 2)[0].Trim();
        }
    }
}